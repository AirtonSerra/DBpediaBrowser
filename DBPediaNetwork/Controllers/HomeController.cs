using DBPediaNetwork.Biz;
using DBPediaNetwork.Models;
using DBPediaNetwork.Models.Authentication;
using DBPediaNetwork.Models.Home;
using DBPediaNetwork.Models.vis.js;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using DBPediaNetwork.Services.DBPedia.Models;
using DBPediaNetwork.Services.DBPedia;

namespace DBPediaNetwork.Controllers
{
    public class HomeController : Controller
    {
        private const int EDGE_LENGTH = 300;
        private const string KEY_NETWORK_DATA = "networkData";
        private const string KEY_DATABASE = "DATABASE";
        private readonly ILogger<HomeController> _logger;
        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");
        private MySqlConnection db;
        private User user = null;
        public HomeController(ILogger<HomeController> logger, MySqlConnection _db)
        {
            _logger = logger;
            db = _db;

        }

        public IActionResult Index()
        {
            HttpContext.Session.Remove("arrColors");
            HttpContext.Session.Remove("arrColorsUsed");
            HomeBiz homeBiz = new HomeBiz(db);
            HomeIndexModel model = new HomeIndexModel(homeBiz.GetAutocompleteSource());


            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public ActionResult Search(SearchFilterViewModel filterModel)
        {
            Data netWorkData = new Data();
            string dbr = filterModel.pesquisa.Split("resource/")[1];

            filterModel.qtdRerouces = (filterModel.qtdRerouces < 99 && filterModel.qtdRerouces > 1) ? filterModel.qtdRerouces : 10;
            filterModel.qtdLiterais = (filterModel.qtdLiterais < 99 && filterModel.qtdLiterais > 1) ? filterModel.qtdLiterais : 10;


            // Adiciona o node principal das pesquisas
            Node nodePrincipal = new Node();
            nodePrincipal.id = netWorkData.getNodeId();
            //nodePrincipal.label = GetResourceLabel(filterModel.pesquisa);
            nodePrincipal.source = filterModel.pesquisa;
            nodePrincipal.clicked = true;
            nodePrincipal.idDad = null;

            netWorkData.nodes.Add(nodePrincipal);

            PerformeQueryBuildData(dbr, ref netWorkData, nodePrincipal, filterModel);

            HttpContext.Session.SetString(KEY_NETWORK_DATA, JsonConvert.SerializeObject(netWorkData));
            return Json(netWorkData);
        }

        [HttpPost]
        public ActionResult ExpandChart(SearchFilterViewModel filterModel)
        {
            var str = HttpContext.Session.GetString(KEY_NETWORK_DATA);
            Data netWorkData = JsonConvert.DeserializeObject<Data>(str);
            string dbr = filterModel.pesquisa.Split("resource/")[1];

            var nodePrincipal = netWorkData.nodes.Where(w => w.source == filterModel.pesquisa).FirstOrDefault();
            nodePrincipal.clicked = true;

            PerformeQueryBuildData(dbr, ref netWorkData, nodePrincipal, filterModel);

            HttpContext.Session.SetString(KEY_NETWORK_DATA, JsonConvert.SerializeObject(netWorkData));
            return Json(netWorkData);
        }

        [HttpPost]
        public ActionResult RemoveNode(int id)
        {
            var str = HttpContext.Session.GetString(KEY_NETWORK_DATA);
            Data netWorkData = JsonConvert.DeserializeObject<Data>(str);

            var nodePrincipal = netWorkData.nodes.Where(w => w.id == id).FirstOrDefault();


            removeNode(nodePrincipal, ref netWorkData);

            HttpContext.Session.SetString(KEY_NETWORK_DATA, JsonConvert.SerializeObject(netWorkData));
            return Json(netWorkData);
        }

        [HttpGet]
        public ActionResult AutoCompleteSearch(string search)
        {
            string query = "select ?x where { " +
                           "?x ?y ?z. " +
                           "filter(regex(lcase(str(?x)), lcase(\"^http://dbpedia.org/resource/" + search + "\"))) } " +
                           "limit 20";

            var results = ExecutSPARQLQuery(query);

            List<string> result = new List<string>();
            string[] arrAux = { };

            if (results != null)
            {
                foreach (SparqlResult item in results) // Traz apenas resources
                {
                    arrAux = item.ToString().Split("?x = http://dbpedia.org/resource/");
                    if (arrAux.Length > 1)
                    {
                        result.Add(arrAux[1].Trim());
                    }
                }
            }

            return Json(result);
        }

        private void removeNode(Node node, ref Data netWorkData)
        {
            foreach (var item in netWorkData.nodes.Where(w => w.idDad == node.id).ToList())
            {
                removeNode(item, ref netWorkData);
            }

            netWorkData.nodes.Remove(node);

            foreach (var item in netWorkData.edges.Where(w => w.to == node.id || w.from == node.id).ToList())
            {

                netWorkData.edges.Remove(item);
            }
        }

        private void PerformeQueryBuildData(string dbr, ref Data netWorkData, Node nodeDad, SearchFilterViewModel filterModel)
        {
            string color = getColor();
            string aux = string.Empty;
            string query = string.Empty;
            string value = string.Empty;
            string[] arrAux = new string[2];
            SparqlResultSet results = new SparqlResultSet();
            List<ResultMainQuerySparqlModel> strDataBase = new List<ResultMainQuerySparqlModel>();
            List<ResultMainQuerySparqlModel> lstResources = new List<ResultMainQuerySparqlModel>();
            List<ResultMainQuerySparqlModel> lstLiterais = new List<ResultMainQuerySparqlModel>();
            List<Node> dbNewNodes = new List<Node>();
            Node node = new Node();
            List<Node> dbNodes = new List<Node>();
            HomeBiz homeBiz = new HomeBiz(db);
            int? dbIdNodeDad = null;

            ResourceResult resources = new ResourceResult();
            LiteralsResult literals = new LiteralsResult();

            string ssUser = HttpContext.Session.GetString(DBPediaNetwork.Controllers.AuthenticationController.SESSION_KEY_USER);
            if (!String.IsNullOrEmpty(ssUser))
            {
                user = JsonConvert.DeserializeObject<User>(ssUser);
            }

            // Consulta se este dbr já está registrado no banco e traz seus filhos, se existirem.
            dbNodes = homeBiz.GetNodes(dbr);

            // Se não houver dados no banco ou o usuário solicitar o refresh dos dados.
            if (dbNodes.Count > 0 && !filterModel.refresh)
            {
                if (String.IsNullOrEmpty(nodeDad.label))
                {
                    nodeDad.label = homeBiz.GetLabelNode(nodeDad);
                }

                var dbLstResources = dbNodes.Where(w => w.isResource).ToList().Take(filterModel.qtdRerouces);
                var dbLstLiterais = dbNodes.Where(w => !w.isResource).ToList().Take(filterModel.qtdLiterais);

                foreach (var item in dbLstResources)
                {
                    item.id = netWorkData.getNodeId();
                    item.color = color;
                    item.idDad = nodeDad.id;

                    netWorkData.nodes.Add(item);
                    netWorkData.edges.Add(new Edge
                    {
                        from = item.id,
                        to = nodeDad.id,
                        length = EDGE_LENGTH,
                        color = node.color
                    });
                }

                foreach (var item in dbLstLiterais)
                {
                    item.id = netWorkData.getNodeId();
                    item.color = color;
                    item.idDad = nodeDad.id;
                    item.shape = "box";
                    node.clicked = true;

                    netWorkData.nodes.Add(item);
                    netWorkData.edges.Add(new Edge
                    {
                        from = item.id,
                        to = nodeDad.id,
                        length = EDGE_LENGTH,
                        color = node.color,
                        label = GetEdgeLabel(new ResultMainQuerySparqlModel { property = item.source })
                    });
                }
            }
            else
            {
                if (String.IsNullOrEmpty(nodeDad.label))
                {
                    nodeDad.label = GetResourceLabel(nodeDad.source);
                }

                resources = Api.GetResources(NormalizaDbr(dbr), filterModel.qtdRerouces);
                literals = Api.GetLiterals(NormalizaDbr(dbr), filterModel.qtdLiterais);


                if (resources.success && (resources?.results?.bindings?.Count() ?? 0) > 0)
                {
                    for (int i = 0; i < resources?.results?.bindings.Count(); i++)
                    {
                        node = new Node();
                        node.id = netWorkData.getNodeId();
                        node.label = resources?.results?.bindings[i].label.value;
                        node.source = resources?.results?.bindings[i].value.value;
                        node.color = color;
                        node.idDad = nodeDad.id;
                        node.isResource = true;

                        dbNewNodes.Add(node);
                        netWorkData.nodes.Add(node);
                        netWorkData.edges.Add(new Edge
                        {
                            from = node.id,
                            to = nodeDad.id,
                            length = EDGE_LENGTH,
                            color = node.color
                        });
                    }
                }

                if (literals.success && (literals?.results?.bindings?.Count() ?? 0) > 0)
                {
                    for (int i = 0; i < literals?.results?.bindings.Count(); i++)
                    {
                        node = new Node();
                        node.id = netWorkData.getNodeId();
                        node.label = GetLiteralLabel(literals?.results?.bindings[i].label.value);
                        node.source = literals?.results?.bindings[i].value.value;
                        node.color = color;
                        node.idDad = nodeDad.id;
                        node.shape = "box";
                        node.clicked = true;
                        node.isResource = false;

                        dbNewNodes.Add(node);
                        netWorkData.nodes.Add(node);
                        netWorkData.edges.Add(new Edge
                        {
                            from = node.id,
                            to = nodeDad.id,
                            length = EDGE_LENGTH,
                            color = node.color,
                            label = GetEdgeLabel(literals?.results?.bindings[i].value.value)
                        });
                    }
                }

                // Insere os novos Nodes no banco.
                if (nodeDad.idDad == null)
                {
                    dbIdNodeDad = homeBiz.InsertNode(nodeDad);
                }
                else
                {
                    dbIdNodeDad = homeBiz.GetNodeDbID(nodeDad);
                }


                if (dbIdNodeDad != null)
                {
                    foreach (var item in dbNewNodes)
                    {
                        homeBiz.InsertNodeChild(item, dbIdNodeDad);
                    }
                }

            }

            if (dbIdNodeDad == null)
            {
                dbIdNodeDad = homeBiz.GetNodeDbID(nodeDad);
            }

            // Registra no banco que o node foi clickado.
            homeBiz.RisterPopularNode(dbIdNodeDad.Value, user);

            db.Close();
            db.Dispose();

            netWorkData.nodes.Where(w => w.id == nodeDad.id).FirstOrDefault().color = color;
        }

        private string GetResourceLabel(string dbr)
        {
            string label = dbr;
            if (label.Contains("resource"))
            {
                string query = "select ?label " +
                               "where { " +
                               "dbr:" + NormalizaDbr(dbr.Split("resource/")[1]) + " rdfs:label ?label . " +
                               "}";

                SparqlResultSet results = ExecutSPARQLQuery(query);
                if (results != null && (results?.Where(w => w.ToString().Contains("@en")).Count() ?? 0) > 0)
                {
                    var labelResult = results.Where(w => w.ToString().Contains("@en")).FirstOrDefault();
                    label = labelResult.ToString().Replace("?label =", "").Replace("@en", "");
                }
                else
                {
                    label = dbr.Split("resource/")[1];
                }

            }
            return label;
        }

        private string GetLiteralLabel(ResultMainQuerySparqlModel result)
        {
            var aux = result.value;
            if (aux.Contains("@"))
            {
                aux = aux.Split("@")[0];
            }

            return aux.Trim();
        }

        private string GetLiteralLabel(string result)
        {
            var aux = result;
            if (aux.Contains("@"))
            {
                aux = aux.Split("@")[0];
            }

            return aux.Trim();
        }

        private string GetEdgeLabel(ResultMainQuerySparqlModel result)
        {
            var aux = result.property.Split("/property/")[1];

            return aux.Trim();
        }

        private string GetEdgeLabel(string result)
        {
            var aux = result.Split("/property/")[1];

            return aux.Trim();
        }

        private SparqlResultSet ExecutSPARQLQuery(string query)
        {
            //Use the DBPedia SPARQL endpoint with the default Graph set to DBPedia
            SparqlResultSet results = null;
            try
            {
                endpoint.Timeout = 80000;
                results = endpoint.QueryWithResultSet(query);
            }
            catch (Exception e)
            {
                var teste = e.Message;
            }
            return results;
        }

        private string getColor()
        {
            string color = string.Empty;
            List<string> arrColors = new List<string> { "#718baf", "#97C2FC", "#FB7E81", "#7BE141", "#6E6EFD", "#C2FABC", "#FFA807", "#fd6ee5" };
            List<string> arrColorsUsed = new List<string>();

            var arr1 = HttpContext.Session.GetString("arrColors");
            var arr2 = HttpContext.Session.GetString("arrColorsUsed");

            if (arr1 != null && arr2 != null)
            {
                arrColors = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("arrColors"));
                arrColorsUsed = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("arrColorsUsed"));
            }

            if (arrColors.Count == 0)
            {
                arrColors = arrColorsUsed.GetRange(0, arrColorsUsed.Count);
                arrColorsUsed.Clear();
            }
            Random rnd = new Random();
            int index = rnd.Next(arrColors.Count());
            color = arrColors[index];
            arrColors.Remove(color);
            arrColorsUsed.Add(color);

            HttpContext.Session.SetString("arrColors", JsonConvert.SerializeObject(arrColors));
            HttpContext.Session.SetString("arrColorsUsed", JsonConvert.SerializeObject(arrColorsUsed));

            return color;
        }

        private string NormalizaDbr(string query)
        {
            return query.Replace(",", "\\,").Replace("(", "\\(").Replace(")", "\\)");
        }
    }
}
