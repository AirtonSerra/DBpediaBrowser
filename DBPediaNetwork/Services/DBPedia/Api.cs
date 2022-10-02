using DBPediaNetwork.Helpers.Communication;
using System.Collections.Generic;
using System;
using DBPediaNetwork.Services.DBPedia.Models;

namespace DBPediaNetwork.Services.DBPedia
{
    public class Api
    {
        private static string _baseURL;
        private static string BaseURL
        {
            get
            {
                if (string.IsNullOrEmpty(_baseURL))
                {
                    _baseURL = "https://dbpedia.org/sparql";
                }

                return _baseURL;
            }
        }


        public static ResourceResult GetResources(string dbr, int? limit)
        {
            ResourceResult modelResult;
            string query = "select distinct ?value ?label " +
                           "where { " +
                           "dbr:" + dbr + " ?property ?value . " +
                           "optional{ " +
                           "?value rdfs:label ?label. " +
                           "} " +
                           "FILTER ( contains(str(?value), \"resource\") ) " +
                           "FILTER(langMatches(lang(?label), \"EN\"))" +
                           "FILTER ( ?value not in ( rdf:type ) ) " +
                           "} " +
                           (limit != null? $"LIMIT {limit}" : string.Empty);

            try
            {
                List<HttpParams> lstParams = new List<HttpParams>() { new HttpParams() { key = "query", value = query }, new HttpParams() { key = "format", value = "application/sparql-results+json" } };

                modelResult = Comunication.doGetRequest<ResourceResult>(BaseURL, lstParams);
            }
            catch (Exception ex)
            {
                modelResult = new ResourceResult
                {
                    success = false,
                    detail = ex.Message,
                    message = "Ocorreu um erro ao realizar a consulta no DBPedia."
                };
            }

            return modelResult;
        }

        public static LiteralsResult GetLiterals(string dbr, int limit)
        {
            LiteralsResult modelResult;
            string query = "SELECT DISTINCT ?value ?label " +
                           "WHERE { " +
                           "dbr:" + dbr + " ?value ?label . " +
                           "FILTER(STRSTARTS(STR(?value), \"http://dbpedia.org/property\") || STRSTARTS(STR(?value), \"http://dbpedia.org/ontology\")) " +
                           "FILTER(isLiteral(?label)  && langMatches(lang(?label), \"EN\"))" +
                           "FILTER ( ?value not in ( rdf:type ) ) " +
                           "} " +
                           (limit != null ? $"LIMIT {limit}" : string.Empty);

            try
            {
                List<HttpParams> lstParams = new List<HttpParams>() { new HttpParams() { key = "query", value = query }, new HttpParams() { key = "format", value = "application/sparql-results+json" } };

                modelResult = Comunication.doGetRequest<LiteralsResult>(BaseURL, lstParams);
            }
            catch (Exception ex)
            {
                modelResult = new LiteralsResult
                {
                    success = false,
                    detail = ex.Message,
                    message = "Ocorreu um erro ao realizar a consulta no DBPedia."
                };
            }

            return modelResult;
        }
    }
}
