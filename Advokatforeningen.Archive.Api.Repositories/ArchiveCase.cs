using Advokatforeningen.Archive.Api.Core;
using Advokatforeningen.Archive.Api.Model;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using static Advokatforeningen.Archive.Api.Repositories.Utilities.Constants;

namespace Advokatforeningen.Archive.Api.Repositories
{
    public class ArchiveCase : IArchiveCase
    {
        #region public methods

        public string GetCaseFolderDetails(int caseId, ArchiveCredentialModel objArchiveModel)
        {
            string siteUrl = objArchiveModel.BaseSiteUrl;
            try
            {
                Uri uri = new Uri(objArchiveModel.BaseSiteUrl);
                string relativeUrl = uri.AbsolutePath;

                if (objArchiveModel.BaseSiteUrl.EndsWith("/", StringComparison.Ordinal))
                    objArchiveModel.BaseSiteUrl += KeywordApi;
                else
                    objArchiveModel.BaseSiteUrl += "/" + KeywordApi;

                string formDigestValue, libraryName = string.Empty, caseFolderName = string.Empty;
                var restClient = RestClientObj(objArchiveModel.BaseSiteUrl, objArchiveModel.Username, objArchiveModel.Password, out formDigestValue);

                int localeId = GetLocaleId(restClient);

                if (Equals(localeId, NorwegianNoLocaleId))
                {
                    libraryName = DocumentLibraryNo;
                    caseFolderName = CaseFolderNameNo;
                }
                else if (Equals(localeId, EnglishUsLocaleId))
                {
                    libraryName = DocumentLibraryEn;
                    caseFolderName = CaseFolderNameEn;
                }

                string folderServerRelativeUrl = string.Concat(relativeUrl, libraryName, "/", caseFolderName, "/", caseId);

                RestRequest request = new RestRequest("web/GetFolderByServerRelativeUrl('" + folderServerRelativeUrl + "')/ListItemAllFields", Method.GET)
                {
                    RequestFormat = DataFormat.Json
                };

                request.AddHeader(Accept, AcceptHeaderVal);
                IRestResponse response = restClient.Execute(request);
                string content = response.Content;

                JObject jobj = JObject.Parse(content);
                string caseFolderDetails = Convert.ToString(jobj["d"]["Id"]);

                objArchiveModel.BaseSiteUrl = siteUrl;

                return string.IsNullOrEmpty(caseFolderDetails) ? CreateCaseFolder(caseId, objArchiveModel) : content;
            }
            catch (Exception)
            {
                objArchiveModel.BaseSiteUrl = siteUrl;
                return CreateCaseFolder(caseId, objArchiveModel);
            }
        }

        public string CreateCaseFolder(int caseId, ArchiveCredentialModel archiveModel)
        {
            //string siteUrl = archiveModel.BaseSiteUrl;

            Uri uri = new Uri(archiveModel.BaseSiteUrl);
            string relativeUrl = uri.AbsolutePath;

            if (archiveModel.BaseSiteUrl.EndsWith("/", StringComparison.Ordinal))
                archiveModel.BaseSiteUrl += KeywordApi;
            else
                archiveModel.BaseSiteUrl += "/" + KeywordApi;

            string formDigestValue, libraryName = string.Empty, caseFolderName = string.Empty;
            var restClient = RestClientObj(archiveModel.BaseSiteUrl, archiveModel.Username, archiveModel.Password, out formDigestValue);

            int localeId = GetLocaleId(restClient);

            if (Equals(localeId, NorwegianNoLocaleId))
            {
                libraryName = DocumentLibraryNo;
                caseFolderName = CaseFolderNameNo;
            }
            else if (Equals(localeId, EnglishUsLocaleId))
            {
                libraryName = DocumentLibraryEn;
                caseFolderName = CaseFolderNameEn;
            }

            string folderServerRelativeUrl = string.Concat(relativeUrl, libraryName, "/", caseFolderName, "/", caseId);

            var body = string.Concat("{'__metadata':{'type':'SP.Folder'},",
                                     "'ServerRelativeUrl':'" + folderServerRelativeUrl + "'}");

            RestRequest request = new RestRequest("Web/Folders", Method.POST)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddHeader(Accept, AcceptHeaderVal);
            request.AddHeader(RequestDigest, formDigestValue);
            request.AddHeader(ContentType, AcceptHeaderVal);
            request.AddParameter(AcceptHeaderVal, body, ParameterType.RequestBody);
            //IRestResponse response = restClient.Execute(request);
            restClient.Execute(request);
            //string result = response.Content;
            string resultData = string.Empty;

            //get case folder id
            ItemDetailModel caseFolderDetailsObj = GetItemDetailsByServerRelativeUrl(folderServerRelativeUrl, restClient);
            int caseFolderId = caseFolderDetailsObj.Id;
            string breakInheritanceResponse = BreakChildFolderInheritance(archiveModel, libraryName, caseFolderId);

            var ownersGroup = Convert.ToString((GetTargetGroupId(archiveModel, OwnersGroup)));
            var readersGroup = Convert.ToString((GetTargetGroupId(archiveModel, ReadersGroup)));

            if (breakInheritanceResponse.ToLower().Equals("OK".ToLower()))
            {
                AssignOwnerPermissions(libraryName, caseFolderId, ownersGroup, Fullcontrol, archiveModel);
                AssignFolderPermissions(libraryName, caseFolderId, readersGroup, Read, archiveModel);

                CreateCaseChildFolder(folderServerRelativeUrl, libraryName, archiveModel);
                resultData = "{\"response\":\"All folders under case folder created successfully\"}";
            }

            return resultData;
        }

        private static ItemDetailModel GetItemDetailsByServerRelativeUrl(string folderServerRelativeUrl, RestClient restClient)
        {
            RestRequest request = new RestRequest("web/GetFolderByServerRelativeUrl('" + folderServerRelativeUrl + "')/ListItemAllFields?$select=id,OData__dlc_DocId", Method.GET)//?$select=id
            {
                RequestFormat = DataFormat.Json
            };
            request.AddHeader(Accept, AcceptHeaderVal);
            IRestResponse response = restClient.Execute(request);
            string content = response.Content;
            JObject jobj = JObject.Parse(content);

            ItemDetailModel listItemDetail = null;
            var metadataObj = jobj["d"]["__metadata"];
            if (!ReferenceEquals(metadataObj, null))
            {
                listItemDetail = new ItemDetailModel
                {
                    Id = Convert.ToInt32(jobj["d"]["Id"]),
                    ETag = Convert.ToString(metadataObj["etag"]),
                    ListItemEntityTypeFullName = Convert.ToString(metadataObj["type"]),
                    DocumentName = Convert.ToString(jobj["d"]["OData__dlc_DocId"])
                };
            }
            return listItemDetail;
        }

        private string BreakChildFolderInheritance(ArchiveCredentialModel archiveModel, string listTitle, int listItemId)
        {
            string formDigestValue;
            var restClient = RestClientObj(archiveModel.BaseSiteUrl, archiveModel.Username, archiveModel.Password, out formDigestValue);

            RestRequest request = new RestRequest("web/lists/GetByTitle('" + listTitle + "')/getItemById(" + listItemId + ")/breakroleinheritance", Method.POST)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddHeader(Accept, AcceptHeaderVal);
            request.AddHeader(RequestDigest, formDigestValue);
            IRestResponse response = restClient.Execute(request);

            if (Equals((HttpStatusCode)response.StatusCode, HttpStatusCode.OK))
            {
                return "OK";
            }
            return string.Empty;
            //return Convert.ToString(response.StatusCode);
        }

        private void CreateCaseChildFolder(string folderServerRelativeUrl, string libraryName, ArchiveCredentialModel archiveModel)
        {
            string formDigestValue;
            var restClient = RestClientObj(archiveModel.BaseSiteUrl, archiveModel.Username, archiveModel.Password, out formDigestValue);

            List<string> templateFolders = new List<string>
            {
                TempUtvalgLib,
                TempNemndLib,
                TempSaksdokumenter,
                TempBeslutning,
                TempSladdetBeslutning
            };

            foreach (string folderTitle in templateFolders)
            {
                var body = string.Concat(
                    "{'__metadata':{'type':'SP.Folder'},",
                    "'ServerRelativeUrl':'" + folderServerRelativeUrl + "/" + folderTitle + "'}");

                RestRequest request = new RestRequest("Web/Folders", Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddHeader(Accept, AcceptHeaderVal);
                request.AddHeader(RequestDigest, formDigestValue);
                request.AddHeader(ContentType, AcceptHeaderVal);
                request.AddParameter(AcceptHeaderVal, body, ParameterType.RequestBody);
                //IRestResponse response = restClient.Execute(request);
                restClient.Execute(request);
                //string result = response.Content;

                ItemDetailModel caseFolderDetailsObj = GetItemDetailsByServerRelativeUrl(folderServerRelativeUrl + "/" + folderTitle, restClient);
                int caseFolderId = caseFolderDetailsObj.Id;
                BreakChildFolderInheritance(archiveModel, libraryName, caseFolderId);
                SetFolderPermissions(folderTitle, libraryName, caseFolderId, archiveModel);
            }
            //return "Child folder created ";
        }

        private void SetFolderPermissions(string folderTitle, string libraryName, int caseFolderId, ArchiveCredentialModel archiveModel)
        {
            //string targetSiteUrl = archiveModel.BaseSiteUrl;

            var ownerGroup = Convert.ToString((GetTargetGroupId(archiveModel, OwnersGroup)));

            if (!folderTitle.ToLower().Equals(TempSladdetBeslutning.ToLower()))
            {
                AssignOwnerPermissions(libraryName, caseFolderId, ownerGroup, Fullcontrol, archiveModel);
            }

            //string result = string.Empty;

            if (folderTitle.ToLower().Equals(TempNemndLib.ToLower()))
            {
                var nemndRead = Convert.ToString(GetTargetGroupId(archiveModel, NemndReaderGroup));
                var nemndContribute = Convert.ToString(GetTargetGroupId(archiveModel, NemndContributeGroup));
                //result += AssignFolderPermissions(libraryName, caseFolderId, nemndContribute, Contribute, archiveModel);
                //result += AssignFolderPermissions(libraryName, caseFolderId, nemndRead, Read, archiveModel);
                AssignFolderPermissions(libraryName, caseFolderId, nemndContribute, Contribute, archiveModel);
                AssignFolderPermissions(libraryName, caseFolderId, nemndRead, Read, archiveModel);
            }

            if (folderTitle.ToLower().Equals(TempUtvalgLib.ToLower()))
            {
                var utvalgContribute = Convert.ToString(GetTargetGroupId(archiveModel, UtvalgContributeGroup));
                var utvalgRead = Convert.ToString(GetTargetGroupId(archiveModel, UtvalgReaderGroup));
                //result += AssignFolderPermissions(libraryName, caseFolderId, utvalgContribute, Contribute, archiveModel);
                //result += AssignFolderPermissions(libraryName, caseFolderId, utvalgRead, Read, archiveModel);
                AssignFolderPermissions(libraryName, caseFolderId, utvalgContribute, Contribute, archiveModel);
                AssignFolderPermissions(libraryName, caseFolderId, utvalgRead, Read, archiveModel);
            }
            //return string.Empty;
        }

        private static string AssignFolderPermissions(string listTitle, int listItemId, string groupId, string roleDefiId, ArchiveCredentialModel archiveModel)
        {
            string formDigestValue;
            var restClient = RestClientObj(archiveModel.BaseSiteUrl, archiveModel.Username, archiveModel.Password, out formDigestValue);

            RestRequest request = new RestRequest("web/lists/getbytitle('" + listTitle + "')/Items(" + listItemId + ")/roleassignments/addroleassignment(principalid='" + groupId + "',roledefid='" + roleDefiId + "')", Method.POST)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddHeader(Accept, AcceptHeaderVal);
            request.AddHeader(ContentType, AcceptHeaderVal);
            request.AddHeader(RequestDigest, formDigestValue);
            IRestResponse response = restClient.Execute(request);
            return response.Content;
        }

        private static void AssignOwnerPermissions(string listTitle, int listItemId, string groupId, string roleDefiId, ArchiveCredentialModel archiveModel)
        {
            string formDigestValue;
            var restClient = RestClientObj(archiveModel.BaseSiteUrl, archiveModel.Username, archiveModel.Password, out formDigestValue);

            RestRequest request = new RestRequest("web/lists/getbytitle('" + listTitle + "')/Items(" + listItemId + ")/roleassignments/addroleassignment(principalid='" + groupId + "',roledefid='" + roleDefiId + "')", Method.POST)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddHeader(Accept, AcceptHeaderVal);
            request.AddHeader(ContentType, AcceptHeaderVal);
            request.AddHeader(RequestDigest, formDigestValue);
            //IRestResponse response = restClient.Execute(request);
            restClient.Execute(request);
            //return response.Content;
        }

        public List<ArchiveCaseDocumentDetailModel> GetDocumentsByCaseIdDocumentId(string caseIdDocumentId, ArchiveCredentialModel archiveModel)
        {
            if (archiveModel.BaseSiteUrl.EndsWith("/", StringComparison.Ordinal))
                archiveModel.BaseSiteUrl += KeywordApi;
            else
                archiveModel.BaseSiteUrl += "/" + KeywordApi;

            string formDigestValue, libraryName = string.Empty;
            var restClient = RestClientObj(archiveModel.BaseSiteUrl, archiveModel.Username, archiveModel.Password, out formDigestValue);

            int localeId = GetLocaleId(restClient);

            if (Equals(localeId, NorwegianNoLocaleId))
            {
                libraryName = DocumentLibraryNo;
            }
            else if (Equals(localeId, EnglishUsLocaleId))
            {
                libraryName = DocumentLibraryEn;
            }

            RestRequest request;
            List<ArchiveCaseDocumentDetailModel> documents = null;
            const string startString = "ADOK-";
            if (caseIdDocumentId.ToLower().StartsWith(startString.ToLower(), StringComparison.Ordinal))
            {
                request = new RestRequest("web/lists/getbytitle('" + libraryName + "')/Items?$select=RegisteredDate,Authority,DocumentStatus,CaseNumber,Origin,PCategory,CategoryOther,PDescription,ContradictionID,DecisionID,PartyID,OwnerID,OData__dlc_DocIdUrl&$filter=_dlc_DocId eq '" + caseIdDocumentId + "'", Method.GET)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddHeader(Accept, AcceptHeaderVal);
                IRestResponse response = restClient.Execute(request);
                string content = response.Content;

                JObject jobj = JObject.Parse(content);
                var resultsArray = jobj["d"]["results"];

                if (resultsArray.Any())
                {
                    documents = new List<ArchiveCaseDocumentDetailModel>
                                {
                                    (from value in resultsArray.Children()
                                        select new ArchiveCaseDocumentDetailModel
                                        {
                                            RegisteredDate = (DateTime)value["RegisteredDate"],//value["RegisteredDate"].HasValues ? (DateTime)value["RegisteredDate"] : default(DateTime),
                                            Authority = value["Authority"].HasValues ? (string)value["Authority"] : string.Empty,
                                            DocumentStatus = (string)value["DocumentStatus"],
                                            CaseNumber = value["CaseNumber"].HasValues ? (int)value["CaseNumber"] : 0,
                                            Origin = (string)value["Origin"],
                                            Category = (string)value["PCategory"],
                                            CategoryOther = value["CategoryOther"].HasValues ? (string)value["CategoryOther"] : String.Empty,
                                            Description = value["PDescription"].HasValues ? (string)value["PDescription"] : String.Empty,
                                            ContradictionId = (string)value["ContradictionID"],
                                            DecisionId = (string)value["DecisionID"],
                                            PartyId = (string)value["PartyID"],
                                            OwnerId = (string)value["OwnerID"],
                                            CaseDocumentTitle = (string)value["OData__dlc_DocIdUrl"]["Description"],
                                            CaseDocumentUrl = (string)value["OData__dlc_DocIdUrl"]["Url"],
                                            ETag = (string)value["__metadata"]["etag"]
                                        }
                                    ).First()
                                };
                }
            }
            else
            {
                request = new RestRequest("web/lists/getbytitle('" + libraryName + "')/Items?$select=RegisteredDate,Authority,DocumentStatus,CaseNumber,Origin,PCategory,CategoryOther,PDescription,ContradictionID,DecisionID,PartyID,OwnerID,OData__dlc_DocIdUrl&$filter=CaseNumber eq " + caseIdDocumentId, Method.GET)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddHeader(Accept, AcceptHeaderVal);
                request.AddHeader(ContentType, AcceptHeaderVal);
                IRestResponse response = restClient.Execute(request);
                string content = response.Content;

                JObject jobj = JObject.Parse(content);
                var resultsArray = jobj["d"]["results"];

                if (resultsArray.Any())
                {
                    documents = (from value in resultsArray.Children()
                                 select new ArchiveCaseDocumentDetailModel
                                 {
                                     RegisteredDate = (DateTime)value["RegisteredDate"],//value["RegisteredDate"].HasValues ? (DateTime)value["RegisteredDate"] : default(DateTime),
                                     Authority = value["Authority"].HasValues ? (string)value["Authority"] : string.Empty,
                                     DocumentStatus = (string)value["DocumentStatus"],
                                     CaseNumber = value["CaseNumber"].HasValues ? (int)value["CaseNumber"] : 0,
                                     Origin = (string)value["Origin"],
                                     Category = (string)value["PCategory"],
                                     CategoryOther = value["CategoryOther"].HasValues ? (string)value["CategoryOther"] : String.Empty,
                                     Description = value["PDescription"].HasValues ? (string)value["PDescription"] : String.Empty,
                                     ContradictionId = (string)value["ContradictionID"],
                                     DecisionId = (string)value["DecisionID"],
                                     PartyId = (string)value["PartyID"],
                                     OwnerId = (string)value["OwnerID"],
                                     CaseDocumentTitle = (string)value["OData__dlc_DocIdUrl"]["Description"],
                                     CaseDocumentUrl = (string)value["OData__dlc_DocIdUrl"]["Url"],
                                     ETag = (string)value["__metadata"]["etag"]
                                 }
                                ).ToList();
                }
            }

            return documents;
        }

        public string UploadDocumentInCaseFolder(int caseId, ArchiveCaseFileModel fileDataObj, ArchiveCredentialModel archiveModel)
        {
            string extension = Path.GetExtension(fileDataObj.FileName);
            if (!string.IsNullOrEmpty(extension))
            {
                Match match = Regex.Match(extension, @".*\.(xls|xlsx|jpg|jpeg|gif|png|doc|docx|pdf|pptx|ppt|txt)?$",
                    RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    Uri uri = new Uri(archiveModel.BaseSiteUrl);
                    string relativeUrl;
                    if (!archiveModel.BaseSiteUrl.Contains(KeywordApi))
                    {
                        relativeUrl = uri.AbsolutePath;
                        if (archiveModel.BaseSiteUrl.EndsWith("/", StringComparison.Ordinal))
                            archiveModel.BaseSiteUrl += KeywordApi;
                        else
                            archiveModel.BaseSiteUrl += "/" + KeywordApi;
                    }
                    else
                    {
                        Uri uri1 = new Uri(uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments.Last().Length));
                        relativeUrl = uri1.AbsolutePath;
                    }
                    string formDigestValue, libraryName = string.Empty, caseFolderName = string.Empty, documentStatusValue = string.Empty;
                    var restClient = RestClientObj(archiveModel.BaseSiteUrl, archiveModel.Username, archiveModel.Password, out formDigestValue);

                    int localeId = GetLocaleId(restClient);

                    if (Equals(localeId, NorwegianNoLocaleId))
                    {
                        libraryName = DocumentLibraryNo;
                        caseFolderName = CaseFolderNameNo;
                        documentStatusValue = DocumentStatusDefaultValNo;
                    }
                    else if (Equals(localeId, EnglishUsLocaleId))
                    {
                        libraryName = DocumentLibraryEn;
                        caseFolderName = CaseFolderNameEn;
                        documentStatusValue = DocumentStatusDefaultValEn;
                    }

                    string folderServerRelativeUrl = string.Concat(relativeUrl, libraryName, "/", caseFolderName, "/", caseId, "/Saksdokumenter");

                    //RestRequest request = null;
                    RestRequest request = new RestRequest("web/GetFolderByServerRelativeUrl('" + folderServerRelativeUrl + "')/files/add(url='" + fileDataObj.FileName + "',overwrite='true')", Method.POST)
                    {
                        RequestFormat = DataFormat.Json
                    };
                    request.AddHeader(Accept, AcceptHeaderVal);
                    request.AddHeader(ContentType, AcceptHeaderVal);
                    request.AddHeader(RequestDigest, formDigestValue);
                    request.AddHeader(BinaryStringRequestBody, "true");
                    request.AddParameter(AcceptHeaderVal, fileDataObj.FileData, ParameterType.RequestBody);
                    //IRestResponse response = restClient.Execute(request);
                    restClient.Execute(request);

                    //JObject jobj = JObject.Parse(response.Content);

                    string serverRelativeUrlOfFile = string.Concat(folderServerRelativeUrl, "/", fileDataObj.FileName);
                    ItemDetailModel listItemDetailObj = GetItemDetailsByServerRelativeUrl(serverRelativeUrlOfFile, restClient);

                    var body = string.Concat(
                                "{",
                                "'__metadata':{'type':'" + listItemDetailObj.ListItemEntityTypeFullName + "'},",
                                "'DocumentStatus':'" + documentStatusValue + "'",
                                "}");

                    request = new RestRequest("web/lists/getbytitle('" + libraryName + "')/items(" + listItemDetailObj.Id + ")", Method.POST)
                    {
                        RequestFormat = DataFormat.Json
                    };
                    request.AddHeader(Accept, AcceptHeaderVal);
                    request.AddHeader(ContentType, AcceptHeaderVal);
                    request.AddHeader("X-HTTP-Method", Method.MERGE.ToString());
                    request.AddHeader("IF-MATCH", listItemDetailObj.ETag);
                    request.AddHeader(RequestDigest, formDigestValue);
                    request.AddParameter(AcceptHeaderVal, body, ParameterType.RequestBody);
                    //IRestResponse updateResponse = restClient.Execute(request);
                    restClient.Execute(request);

                    string file = Path.GetFileNameWithoutExtension(serverRelativeUrlOfFile),
                           documentName = listItemDetailObj.DocumentName,
                           newPath = serverRelativeUrlOfFile.Replace(file, documentName);

                    return "{\"response\":\"Case Document uploaded successfully at: " + newPath + " and to update metadata call /Documents/{documentId} of type PUT\",\"CaseDocumentName\":\"" + documentName + "\"}";
                }
            }
            return "{\"response\":\"Invalid file Extension " + extension + "\"}";
        }

        private static ItemDetailModel GetItemDetailsByDocumentId(string documentId, string libraryName, RestClient restClient)
        {
            RestRequest request = new RestRequest("web/lists/getbytitle('" + libraryName + "')/Items?$select=ID&$filter=_dlc_DocId eq '" + documentId + "'", Method.GET)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddHeader(Accept, AcceptHeaderVal);
            IRestResponse response = restClient.Execute(request);
            string content = response.Content;
            JObject jobj = JObject.Parse(content);
            ItemDetailModel listItemDetail = null;

            var results = jobj["d"]["results"];
            if (!ReferenceEquals(results, null) && results.Any())
            {
                listItemDetail = (from value in results.Children()
                                  select new ItemDetailModel
                                  {
                                      Id = (int)value["Id"],
                                      ETag = (string)value["__metadata"]["etag"],
                                      ListItemEntityTypeFullName = (string)value["__metadata"]["type"]
                                  }
                                ).First();
            }

            return listItemDetail;
        }

        public string DeleteCaseDocument(string documentId, ArchiveCredentialModel objArchiveModel)
        {
            //string startString = "ADOK";
            //if (documentId.Trim().ToLower().StartsWith(startString.ToLower(), StringComparison.Ordinal))
            //{
            if (objArchiveModel.BaseSiteUrl.EndsWith("/", StringComparison.Ordinal))
                objArchiveModel.BaseSiteUrl += KeywordApi;
            else
                objArchiveModel.BaseSiteUrl += "/" + KeywordApi;

            string formDigestValue, libraryName = string.Empty, documentStatus = string.Empty;
            RestClient restClient = RestClientObj(objArchiveModel.BaseSiteUrl, objArchiveModel.Username, objArchiveModel.Password, out formDigestValue);

            int localeId = GetLocaleId(restClient);

            if (Equals(localeId, NorwegianNoLocaleId))
            {
                libraryName = DocumentLibraryNo;
                documentStatus = DeletedNo;
            }
            else if (Equals(localeId, EnglishUsLocaleId))
            {
                libraryName = DocumentLibraryEn;
                documentStatus = DeletedEn;
            }

            ItemDetailModel listItemDetailObj = GetItemDetailsByDocumentId(documentId, libraryName, restClient);

            //string body = string.Empty;

            if (!ReferenceEquals(listItemDetailObj, null))
            {
                //if (!string.IsNullOrEmpty(deleteFlag))//(!ReferenceEquals(deleteFlag, null))
                //{
                string body = string.Concat(
                        "{",
                        "'__metadata':{'type':'" + listItemDetailObj.ListItemEntityTypeFullName + "'},",
                        "'DocumentStatus':'" + documentStatus + "'",
                        "}");

                RestRequest request = new RestRequest("web/lists/getbytitle('" + libraryName + "')/items(" + listItemDetailObj.Id + ")", Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddHeader(Accept, AcceptHeaderVal);
                request.AddHeader(ContentType, AcceptHeaderVal);
                request.AddHeader("X-HTTP-Method", Method.MERGE.ToString());
                request.AddHeader("IF-MATCH", listItemDetailObj.ETag);
                request.AddHeader(RequestDigest, formDigestValue);
                request.AddParameter(AcceptHeaderVal, body, ParameterType.RequestBody);
                IRestResponse response = restClient.Execute(request);
                return string.IsNullOrEmpty(response.Content) ? "{\"response\":\"Case status updated successfully\"}" : "{\"response\":\"Error- Try again\"}";
                //}
            }
            //if (string.IsNullOrEmpty(body))
            //{
            return "{\"response\":\"Document does not exists\"}";
            //}
            //}
            //return "{\"response\":\"Wrong Pattern entered for Document ID\"}";
        }

        public string UpdateCaseDocumentMetadata(string documentId, ArchiveCaseDocumentModel fileMetadataObj, ArchiveCredentialModel archiveModel)
        {
            if (archiveModel.BaseSiteUrl.EndsWith("/", StringComparison.Ordinal))
                archiveModel.BaseSiteUrl += KeywordApi;
            else
                archiveModel.BaseSiteUrl += "/" + KeywordApi;

            string formDigestValue, libraryName = string.Empty, documentStatusValue = string.Empty;
            RestClient restClient = RestClientObj(archiveModel.BaseSiteUrl, archiveModel.Username, archiveModel.Password, out formDigestValue);

            int localeId = GetLocaleId(restClient);

            if (Equals(localeId, NorwegianNoLocaleId))
            {
                libraryName = DocumentLibraryNo;
                documentStatusValue = DocumentStatusDefaultValNo;
            }
            else if (Equals(localeId, EnglishUsLocaleId))
            {
                libraryName = DocumentLibraryEn;
                documentStatusValue = DocumentStatusDefaultValEn;
            }

            ItemDetailModel listItemDetailObj = GetItemDetailsByDocumentId(documentId, libraryName, restClient);

            //var body = string.Empty;
            //if (listItemDetailObj != null)
            if (!ReferenceEquals(listItemDetailObj, null))
            {
                if (!ReferenceEquals(fileMetadataObj, null))
                {
                    string authority = !string.IsNullOrEmpty(fileMetadataObj.Authority) ? "'Authority':'" + fileMetadataObj.Authority + "'," : string.Empty,
                        //caseNumber = !string.IsNullOrEmpty(Convert.ToString(fileMetadataObj.CaseNumber)) ? "'CaseNumber':'" + fileMetadataObj.CaseNumber + "'," : string.Empty,
                        origin = !string.IsNullOrEmpty(fileMetadataObj.Origin) ? "'Origin':'" + fileMetadataObj.Origin + "'," : string.Empty,
                        category = !string.IsNullOrEmpty(fileMetadataObj.Category) ? "'PCategory':'" + fileMetadataObj.Category + "'," : string.Empty,
                        contradictionId = !string.IsNullOrEmpty(fileMetadataObj.ContradictionId) ? "'ContradictionID':'" + fileMetadataObj.ContradictionId + "'," : string.Empty,
                        decisionId = !string.IsNullOrEmpty(fileMetadataObj.DecisionId) ? "'DecisionID':'" + fileMetadataObj.DecisionId + "'," : string.Empty,
                        partyId = !string.IsNullOrEmpty(fileMetadataObj.PartyId) ? "'PartyID':'" + fileMetadataObj.PartyId + "'," : string.Empty,
                        ownerId = !string.IsNullOrEmpty(fileMetadataObj.OwnerId) ? "'OwnerID':'" + fileMetadataObj.OwnerId + "'," : string.Empty,
                        documentStatus = !string.IsNullOrEmpty(fileMetadataObj.DocumentStatus) ? "'DocumentStatus':'" + fileMetadataObj.DocumentStatus + "'" : "'DocumentStatus':'" + documentStatusValue + "'";

                    string body = string.Concat(
                        "{",
                        "'__metadata':{'type':'" + listItemDetailObj.ListItemEntityTypeFullName + "'},",
                        authority, origin, category, contradictionId,
                        decisionId, partyId, ownerId, documentStatus,
                        "}");

                    RestRequest request =
                        new RestRequest("web/lists/getbytitle('" + libraryName + "')/items(" + listItemDetailObj.Id + ")", Method.POST)
                        {
                            RequestFormat = DataFormat.Json
                        };
                    request.AddHeader(Accept, AcceptHeaderVal);
                    request.AddHeader(ContentType, AcceptHeaderVal);
                    request.AddHeader("X-HTTP-Method", Method.MERGE.ToString());
                    request.AddHeader("IF-MATCH", listItemDetailObj.ETag);
                    request.AddHeader(RequestDigest, formDigestValue);
                    request.AddParameter(AcceptHeaderVal, body, ParameterType.RequestBody);
                    IRestResponse response = restClient.Execute(request);

                    return string.IsNullOrEmpty(response.Content)
                        ? "{\"response\":\"Case file metadata updated successfully\"}"
                        : "{\"response\":\"Error- Try again\"}";
                }
                return "{\"response\":\"No Metadata found to update for " + documentId + ".\"}";
            }
            else
            {
                return "{\"response\":\"Document ID not found.\"}";
            }
        }

        public string CopyCaseDocuments(int sourceCaseId, int destinationCaseId, ArchiveCredentialModel archiveModel, string copyFlag)
        {
            Uri uri = new Uri(archiveModel.BaseSiteUrl);
            string relativeUrl = uri.AbsolutePath;

            if (archiveModel.BaseSiteUrl.EndsWith("/", StringComparison.Ordinal))
                archiveModel.BaseSiteUrl += KeywordApi;
            else
                archiveModel.BaseSiteUrl += "/" + KeywordApi;

            string formDigestValue, libraryName = string.Empty, recordCenterLibrary = string.Empty, caseFolderName = string.Empty;
            var restClient = RestClientObj(archiveModel.BaseSiteUrl, archiveModel.Username, archiveModel.Password, out formDigestValue);

            int localeId = GetLocaleId(restClient);
            if (Equals(localeId, NorwegianNoLocaleId))
            {
                libraryName = DocumentLibraryNo;
                caseFolderName = CaseFolderNameNo;
                recordCenterLibrary = "RecordCenter";
            }
            else if (Equals(localeId, EnglishUsLocaleId))
            {
                libraryName = DocumentLibraryEn;
                caseFolderName = CaseFolderNameEn;
                recordCenterLibrary = "RecordCenter";   // assign norwegian name
            }

            if (!relativeUrl.EndsWith("/", StringComparison.Ordinal))
            {
                relativeUrl += "/";
            }

            string baseServerRelativeUrl = string.Concat(relativeUrl, "@@/"),
                sourceServerRelativeUrl = string.Empty,
                destinationServerRelativeUrl = string.Empty,
                recordCenterCurrentYearLib = string.Empty;

            RestRequest request = null;

            if (copyFlag.ToLower().Equals("RecordCenter".ToLower()))
            {
                recordCenterCurrentYearLib = "Archives1" + DateTime.Now.Year;
                request = new RestRequest("web/lists/getbytitle('" + recordCenterCurrentYearLib + "')/title", Method.GET)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddHeader(Accept, AcceptHeaderVal);

                IRestResponse response = restClient.Execute(request);
                string result = response.Content;
                JObject jobj = JObject.Parse(result);
                string results = string.Empty;
                if (jobj.HasValues)
                {
                    results = Convert.ToString(result.StartsWith("{\"error\"", StringComparison.Ordinal) ? jobj["error"]["code"] : jobj["d"]["Title"]);
                }

                if (results.ToLower().Equals(recordCenterCurrentYearLib.ToLower()))
                {
                    sourceServerRelativeUrl = string.Concat(baseServerRelativeUrl.Replace("@@", recordCenterCurrentYearLib), "##");
                    destinationServerRelativeUrl = string.Concat(baseServerRelativeUrl.Replace("@@", libraryName), caseFolderName, "/##");
                }
                else
                {
                    // TODO: create library, add custom content type and set as default.

                    string body = string.Concat("{'__metadata':{'type':'SP.List'},",
                                 "'AllowContentTypes': true,",
                                 "'ContentTypesEnabled': true,",
                                 "'Description': 'Library to archive documents',",
                                 "'Title':'" + recordCenterCurrentYearLib + "',",
                                 "'BaseTemplate':'{00BFEA71-E717-4E80-AA17-D0C71B360101}'}");

                    request = new RestRequest("web/lists", Method.POST)
                    {
                        RequestFormat = DataFormat.Json
                    };
                    request.AddHeader(Accept, AcceptHeaderVal);
                    request.AddHeader(RequestDigest, formDigestValue);
                    request.AddParameter(AcceptHeaderVal, body, ParameterType.RequestBody);

                    IRestResponse updateResponse = restClient.Execute(request);

                    //body = "'contentTypeId': '0x01010085E008E5914A496797B08340036D67D4'";

                    //request = new RestRequest("web/lists/GetByTitle('" + recordCenterCurrentYearLib + "')/ContentTypes/AddAvailableContentType('0x01010085E008E5914A496797B08340036D67D4')", Method.POST)
                    //{
                    //    RequestFormat = DataFormat.Json
                    //};
                    //request.AddHeader(Accept, AcceptHeaderVal);
                    //request.AddHeader(RequestDigest, formDigestValue);
                    ////request.AddParameter(AcceptHeaderVal, body, ParameterType.RequestBody);

                    //IRestResponse updateResponse1 = restClient.Execute(request);
                }
            }
            else if (copyFlag.ToLower().Equals("NotRecordCenter".ToLower()))
            {
                sourceServerRelativeUrl = string.Concat(baseServerRelativeUrl.Replace("@@", libraryName), caseFolderName, "/##");
                destinationServerRelativeUrl = sourceServerRelativeUrl;
            }

            //string folderServerRelativeUrl = string.Concat(relativeUrl, libraryName, "/", caseFolderName, "/@@/", folderName),

            //string baseServerRelativeUrl = string.Concat(relativeUrl, libraryName, "/", caseFolderName, "/##");
            //string sourceCaseFolderServerRelativeUrl = baseServerRelativeUrl.Replace("##", sourceCaseId.ToString()),
            //       destCaseFolderServerRelativeUrl = baseServerRelativeUrl.Replace("##", destinationCaseId.ToString());

            string sourceCaseFolderServerRelativeUrl = sourceServerRelativeUrl.Replace("##", sourceCaseId.ToString()),
                   destCaseFolderServerRelativeUrl = destinationServerRelativeUrl.Replace("##", destinationCaseId.ToString());

            List<string> folderToCopyData = new List<string>
            {
                TempSaksdokumenter,TempBeslutning
            };
            string caseResult = string.Empty;
            string resultData = string.Empty;
            ItemDetailModel srcCaseFolderDetailVal = GetItemDetailsByServerRelativeUrl(sourceCaseFolderServerRelativeUrl, restClient);
            if (!ReferenceEquals(srcCaseFolderDetailVal, null))
            {
                /* commented by vikas */
                //string srcURL = sourceCaseFolderServerRelativeUrl;
                //string destURL = destCaseFolderServerRelativeUrl;
                ItemDetailModel destCaseFolderDetailVal = GetItemDetailsByServerRelativeUrl(destCaseFolderServerRelativeUrl, restClient);
                if (!ReferenceEquals(destCaseFolderDetailVal, null))
                {
                    foreach (string folderName in folderToCopyData)
                    {
                        string sourceUrl = sourceCaseFolderServerRelativeUrl + "/" + folderName;
                        ItemDetailModel srcCaseFolderDetailVal2 = GetItemDetailsByServerRelativeUrl(sourceUrl, restClient);
                        if (!ReferenceEquals(srcCaseFolderDetailVal2, null))
                        {
                            string destinationUrl = destCaseFolderServerRelativeUrl + "/" + folderName;
                            ItemDetailModel destCaseFolderDetailVal2 = GetItemDetailsByServerRelativeUrl(destinationUrl, restClient);
                            if (!ReferenceEquals(destCaseFolderDetailVal2, null))
                            {
                                //RestRequest request = null;
                                request = new RestRequest("web/GetFolderByServerRelativeUrl('" + sourceUrl + "')/Files?$select=Name", Method.GET)
                                {
                                    RequestFormat = DataFormat.Json
                                };
                                request.AddHeader(Accept, AcceptHeaderVal);

                                IRestResponse response = restClient.Execute(request);
                                string result = response.Content;

                                JObject jobj = JObject.Parse(result);
                                var results = jobj["d"]["results"];

                                if (!ReferenceEquals(results, null) && results.Any())
                                {
                                    List<string> caseFileNameCollection = (from value in results.Children()
                                                                           select Convert.ToString(value["Name"])
                                                                          ).ToList();

                                    int caseFileNameCount = caseFileNameCollection.Count, index;

                                    for (index = 0; index < caseFileNameCount; index++)
                                    {
                                        string srcUrl = sourceUrl, destUrl = destinationUrl;
                                        srcUrl += "/" + caseFileNameCollection[index];
                                        destUrl += "/" + caseFileNameCollection[index];

                                        //  resultData = resultData + "web/GetFileByServerRelativeUrl('" + srcUrl + "')/copyto(strnewurl='" + destUrl + "', boverwrite = true)";

                                        request =
                                            new RestRequest("web/GetFileByServerRelativeUrl('" + srcUrl + "')/copyto(strnewurl='" + destUrl + "', boverwrite = true)", Method.POST)
                                            {
                                                RequestFormat = DataFormat.Json
                                            };
                                        request.AddHeader(Accept, AcceptHeaderVal);
                                        request.AddHeader(RequestDigest, formDigestValue);
                                        IRestResponse caseDocumentresponse = restClient.Execute(request);

                                        JObject jobject = JObject.Parse(caseDocumentresponse.Content);
                                        bool isResponseExists = jobject["d"]["CopyTo"].HasValues;
                                        if (!isResponseExists)
                                        {
                                            caseResult += string.Empty;
                                        }
                                    }
                                    // resultData = resultData + " Data Copied";
                                    if (resultData.Equals("Case Documents copied successfully"))
                                    {
                                        resultData = string.IsNullOrEmpty(caseResult)
                                                                    ? "Case Documents copied successfully"
                                                                    : "sError- Try again";
                                    }
                                    else
                                    {
                                        resultData = string.IsNullOrEmpty(caseResult)
                                                                    ? resultData + "Case Documents copied successfully"
                                                                    : resultData + "Error- Try again";
                                    }
                                }
                                else
                                {
                                    if (resultData.Equals("No Documents"))
                                    {
                                        resultData = "No Documents";
                                    }
                                    else
                                    {
                                        resultData += "No Documents";
                                    }
                                    // return "{\"response\":\"No Documents\"}";
                                }
                            }
                            else
                            {
                                // return "{\"response\":\"Folder Does not exists under Destination Case\"}";
                                resultData = "Folder Does not exists under Destination Case";
                            }
                        }
                        else
                        {
                            //  return "{\"response\":\"Folder Does not exists under Source Case\"}";
                            resultData = "Folder Does not exists under Source Case";
                        }
                    }
                }
                else
                {
                    //return "{\"response\":\"Destination Case Folder Does not exists\"}";
                    resultData = "Destination Case Folder Does not exists";
                }
            }
            else
            {
                resultData += "Source Case Folder Does not exists";
            }

            //return string.IsNullOrEmpty(caseResult)
            //                            ? "{\"response\":\"Case Documents copied successfully\"}"
            //                            : "{\"response\":\"Error- Try again\"}";
            //      return "{\"response\":\"Source Case Folder Does not exists\"}";
            resultData = "{\"response\":\"" + resultData + "\"}";
            return resultData;
        }

        //******************************************************************************************************************************************************************//

        /// <summary>
        /// Set permissions on the "Saksdokumenter" library as per parameters
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="folderName"></param>
        /// <param name="archiveModel"></param>
        /// <returns></returns>
        public string SetPermission(int caseId, string folderName, ArchiveCredentialModel archiveModel)
        {
            if (folderName.ToLower().Equals(NemndResource.ToLower()) || folderName.ToLower().Equals(UtvalgResource.ToLower()))
            {
                Uri uri = new Uri(archiveModel.BaseSiteUrl);
                string relativeUrl = uri.AbsolutePath;

                string targetSiteUrl = archiveModel.BaseSiteUrl;
                if (targetSiteUrl.EndsWith("/", StringComparison.Ordinal))
                    targetSiteUrl += KeywordApi;
                else
                    targetSiteUrl += "/" + KeywordApi;
                archiveModel.BaseSiteUrl = targetSiteUrl;

                string formDigestValue, libraryName = string.Empty, caseFolderName = string.Empty;
                var restClient = RestClientObj(targetSiteUrl, archiveModel.Username, archiveModel.Password, out formDigestValue);

                int localeId = GetLocaleId(restClient);

                if (Equals(localeId, NorwegianNoLocaleId))
                {
                    libraryName = DocumentLibraryNo;
                    caseFolderName = CaseFolderNameNo;
                }
                else if (Equals(localeId, EnglishUsLocaleId))
                {
                    libraryName = DocumentLibraryEn;
                    caseFolderName = CaseFolderNameEn;
                }

                var nemndRead = Convert.ToString(GetTargetGroupId(archiveModel, NemndReaderGroup));
                var nemndContribute = Convert.ToString(GetTargetGroupId(archiveModel, NemndContributeGroup));
                var utvalgContribute = Convert.ToString(GetTargetGroupId(archiveModel, UtvalgContributeGroup));
                var utvalgRead = Convert.ToString(GetTargetGroupId(archiveModel, UtvalgReaderGroup));

                string result = string.Empty;
                //string removeExistingPermissions = string.Empty;

                List<string> templateFolders = new List<string>
                {
                    TempBeslutning,
                    TempSladdetBeslutning,
                    TempSaksdokumenter
                };

                foreach (string folderTitle in templateFolders)
                {
                    string caseFolderServerRelativeUrl = string.Concat(relativeUrl, libraryName, "/", caseFolderName, "/", caseId, "/", folderTitle);
                    ItemDetailModel caseFolderDetailsObj = GetItemDetailsByServerRelativeUrl(caseFolderServerRelativeUrl, restClient);

                    if (ReferenceEquals(caseFolderDetailsObj, null))
                    {
                        return "{\"response\":\"error\"}";
                    }

                    int caseFolderId = caseFolderDetailsObj.Id;

                    if (folderName.ToLower().Equals(NemndResource))
                    {
                        if (folderTitle.ToLower().Equals(TempSaksdokumenter.ToLower()))
                        {
                            //removeExistingPermissions = RemoveFolderPermissions(libraryName, caseFolderId, utvalgRead, archiveModel);
                            //removeExistingPermissions = RemoveFolderPermissions(libraryName, caseFolderId, utvalgContribute, archiveModel);

                            RemoveFolderPermissions(libraryName, caseFolderId, utvalgRead, archiveModel);
                            RemoveFolderPermissions(libraryName, caseFolderId, utvalgContribute, archiveModel);

                            result += AssignFolderPermissions(libraryName, caseFolderId, nemndContribute, Contribute, archiveModel);
                            result += AssignFolderPermissions(libraryName, caseFolderId, nemndRead, Read, archiveModel);
                        }
                        else
                        {
                            //removeExistingPermissions = RemoveFolderPermissions(libraryName, caseFolderId, nemndRead, archiveModel);
                            //removeExistingPermissions = RemoveFolderPermissions(libraryName, caseFolderId, utvalgContribute, archiveModel);

                            RemoveFolderPermissions(libraryName, caseFolderId, nemndRead, archiveModel);
                            RemoveFolderPermissions(libraryName, caseFolderId, utvalgContribute, archiveModel);

                            result += AssignFolderPermissions(libraryName, caseFolderId, nemndContribute, Contribute, archiveModel);
                            result += AssignFolderPermissions(libraryName, caseFolderId, utvalgRead, Read, archiveModel);
                        }
                    }

                    if (folderName.ToLower().Equals(UtvalgResource))
                    {
                        if (folderTitle.ToLower().Equals(TempSaksdokumenter.ToLower()))
                        {
                            //removeExistingPermissions = RemoveFolderPermissions(libraryName, caseFolderId, nemndContribute, archiveModel);
                            //removeExistingPermissions = RemoveFolderPermissions(libraryName, caseFolderId, nemndRead, archiveModel);

                            RemoveFolderPermissions(libraryName, caseFolderId, nemndContribute, archiveModel);
                            RemoveFolderPermissions(libraryName, caseFolderId, nemndRead, archiveModel);

                            result += AssignFolderPermissions(libraryName, caseFolderId, utvalgContribute, Contribute, archiveModel);
                            result += AssignFolderPermissions(libraryName, caseFolderId, utvalgRead, Read, archiveModel);
                        }
                        else
                        {
                            //removeExistingPermissions = RemoveFolderPermissions(libraryName, caseFolderId, utvalgRead, archiveModel);
                            //removeExistingPermissions += RemoveFolderPermissions(libraryName, caseFolderId, nemndContribute, archiveModel);

                            RemoveFolderPermissions(libraryName, caseFolderId, utvalgRead, archiveModel);
                            RemoveFolderPermissions(libraryName, caseFolderId, nemndContribute, archiveModel);

                            result += AssignFolderPermissions(libraryName, caseFolderId, utvalgContribute, Contribute, archiveModel);
                            result += AssignFolderPermissions(libraryName, caseFolderId, nemndRead, Read, archiveModel);
                        }
                    }
                }

                if (result.ToLower().Contains("error"))
                    return "{\"response\":\"error try again later\"}";
                return "{\"response\":\"Permissions assigned successfully for " + folderName + " group.\"}";
            }
            else
            {
                return "{\"response\":\"Wrong Authority name entered\"}";
            }
        }

        private static void RemoveFolderPermissions(string listTitle, int listItemId, string groupId, ArchiveCredentialModel archiveModel)
        {
            string formDigestValue;
            var rc = RestClientObj(archiveModel.BaseSiteUrl, archiveModel.Username, archiveModel.Password, out formDigestValue);

            RestRequest request = new RestRequest("web/lists/getbytitle('" + listTitle + "')/Items(" + listItemId + ")/roleassignments/getbyprincipalid('" + groupId + "')", Method.POST)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddHeader(Accept, AcceptHeaderVal);
            request.AddHeader(ContentType, AcceptHeaderVal);
            request.AddHeader("X-RequestDigest", formDigestValue);
            request.AddHeader("X-HTTP-Method", "DELETE");
            //IRestResponse response = rc.Execute(request);
            rc.Execute(request);
            //string content = response.Content;
            //return Convert.ToString(response.StatusCode);
            //return response.StatusCode;
        }

        #endregion public methods

        #region private methods

        /// <summary>
        /// gets the locale id
        /// </summary>
        /// <param name="rc"></param>
        /// <returns></returns>
        private static int GetLocaleId(RestClient rc)
        {
            RestRequest request = new RestRequest("web/language", Method.GET)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddHeader(Accept, AcceptHeaderVal);
            IRestResponse response = rc.Execute(request);
            string content = response.Content;
            JObject jobj = JObject.Parse(content);
            return Convert.ToInt32(jobj["d"]["Language"]);
        }

        /// <summary>
        /// Get the Rest Client
        /// </summary>
        /// <param name="baseSiteUrl"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="formDigestValue"></param>
        /// <returns></returns>
        private static RestClient RestClientObj(string baseSiteUrl, string user, string password, out string formDigestValue)
        {
            RestClient restClient = new RestClient(baseSiteUrl)
            {
                Authenticator = new NtlmAuthenticator(user, password)
            };

            RestRequest request = new RestRequest("contextinfo?$select=FormDigestValue", Method.POST);
            request.AddHeader(Accept, AcceptHeaderVal);
            request.AddHeader("Body", "");

            string response = restClient.Execute(request).Content;

            JObject jobj = JObject.Parse(response);
            formDigestValue = Convert.ToString(jobj["d"]["GetContextWebInformation"]["FormDigestValue"]);

            return restClient;
        }

        /// <summary>
        /// Get the target group id to assign the permissions
        /// </summary>
        /// <param name="archiveModel"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        private static string GetTargetGroupId(ArchiveCredentialModel archiveModel, string groupName)
        {
            string formDigestValue;
            var restClient = RestClientObj(archiveModel.BaseSiteUrl, archiveModel.Username, archiveModel.Password, out formDigestValue);

            RestRequest request = new RestRequest("web/sitegroups/getbyname('" + groupName.Trim() + "')/id", Method.GET)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddHeader(Accept, AcceptHeaderVal);
            string content = restClient.Execute(request).Content;

            JObject jobj = JObject.Parse(content);
            string groupId = Convert.ToString(jobj["d"]["Id"]);
            return groupId;
        }

        #endregion private methods
    }
}