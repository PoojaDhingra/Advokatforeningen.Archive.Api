using Advokatforeningen.Archive.Api.Core;
using Advokatforeningen.Archive.Api.Model;

//using Advokatforeningen.Archive.Api.Repositories.HelperClasses;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.OData;

namespace Advokatforeningen.Archive.Api.Controllers
{
    /// <summary>
    /// Archive case controller contains all api action methods
    /// </summary>
    [RoutePrefix("Documents")]
    [EnableCors(origins: "http://eimskip", headers: "*", methods: "*")]
    public class ArchiveCaseController : ApiController
    {
        #region private fields

        private readonly ArchiveCredentialModel _objArchiveModel;
        private readonly IArchiveCase _objArchiveCase;

        #endregion private fields

        #region public controller methods

        /// <summary>
        /// Constructor to initialize the resources
        /// </summary>
        /// <remarks>Initializes Interface and Model</remarks>
        /// <param name="objArchiveCase">Interface object</param>
        public ArchiveCaseController(IArchiveCase objArchiveCase) //, ArchiveCredentialModel objArchiveModel
        {
            _objArchiveCase = objArchiveCase;
            _objArchiveModel = new ArchiveCredentialModel
            {
                BaseSiteUrl = Convert.ToString(ConfigurationManager.AppSettings["BaseSiteUrl"]),
                Username = Convert.ToString(ConfigurationManager.AppSettings["UserName"]),
                Password = Convert.ToString(ConfigurationManager.AppSettings["Password"])
            };
        }

        /// <summary>
        /// Open SharePoint case folder, call create if none existent
        /// </summary>
        /// <remarks>
        /// Get case details if already exists, create otherwise.
        /// </remarks>
        /// <param name="caseId">Case Id</param>
        /// <returns>Response code and contents</returns>
        [Route("~/Archives/{caseId:int:min(10000)}")]
        [HttpGet]
        public HttpResponseMessage GetCaseFolderDetails(int caseId)
        {
            try
            {
                JToken json = JObject.Parse(_objArchiveCase.GetCaseFolderDetails(caseId, _objArchiveModel));
                if (ReferenceEquals(json, null))
                {
                    HttpResponseMessage resp = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Content = new StringContent($"No Case with ID = {caseId} found"),
                        ReasonPhrase = "Case Not Found"
                    };
                    throw new HttpResponseException(resp);
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, json);
                return response;
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = ex.Response.StatusCode,
                    Content = ex.Response.Content,
                    ReasonPhrase = ex.Response.ReasonPhrase
                });
            }
            catch (NullReferenceException ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                });
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                });
            }
        }

        /// <summary>
        /// Create single archive folder in SharePoint "Document" library
        /// </summary>
        /// <remarks>
        /// Create case folder with sub-folders and custom security permissions.
        /// </remarks>
        /// <param name="caseId">Case Id</param>
        /// <returns>Response code and contents</returns>
        [Route("~/Archives/{caseId:int:min(10000)}")]
        [HttpPost]
        public HttpResponseMessage CreateCaseFolder(int caseId)
        {
            try
            {
                JToken json = JObject.Parse(_objArchiveCase.CreateCaseFolder(caseId, _objArchiveModel));
                if (ReferenceEquals(json, null))
                {
                    HttpResponseMessage resp = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Ambiguous,
                        Content = new StringContent($"Case with ID = {caseId} cannot be created"),
                        ReasonPhrase = "Case cannot be created"
                    };
                    throw new HttpResponseException(resp);
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, json);
                return response;
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = ex.Response.StatusCode,
                    Content = ex.Response.Content,
                    ReasonPhrase = ex.Response.ReasonPhrase
                });
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                });
            }
        }

        /// <summary>
        /// Sett security on archive to Nemnd/Utvalg.
        /// </summary>
        /// <remarks>
        /// Set security on archive to Nemnd/Utvalg
        /// </remarks>
        /// <param name="caseId">Case Id</param>
        /// <param name="name">Authority Name</param>
        /// <returns>json response</returns>
        [Route("~/Archives/{caseId:int:min(10000)}/{name:alpha}")]  //[Route("~/Archives/{caseId}/{name}")]
        [HttpPut]
        public HttpResponseMessage SetPermission(int caseId, string name)
        {
            try
            {
                JToken json = JObject.Parse(_objArchiveCase.SetPermission(caseId, name.Trim(), _objArchiveModel));

                if (!ReferenceEquals(json, null))
                {
                    string responseData = Convert.ToString(json["response"]);
                    if (responseData.ToLower().Equals("error".ToLower()))
                    {
                        HttpResponseMessage resp = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.NotFound,
                            Content = new StringContent($"Case with title: {caseId} does not exists"),
                            ReasonPhrase = "Case does not exists"
                        };
                        throw new HttpResponseException(resp);
                    }

                    if (responseData.ToLower().Equals("Wrong Authority name entered".ToLower()))
                    {
                        HttpResponseMessage resp = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.NotFound,
                            Content = new StringContent($"Authority with name: {name} does not exists"),
                            ReasonPhrase = "Authority does not exists"
                        };
                        throw new HttpResponseException(resp);
                    }
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, json);
                return response;
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = ex.Response.StatusCode,
                    Content = ex.Response.Content,
                    ReasonPhrase = ex.Response.ReasonPhrase
                });
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                });
            }
        }

        /// <summary>
        /// Get a list of documents, in given case folder.
        /// </summary>
        /// <remarks>
        /// Get all case related documents.
        /// </remarks>
        /// <param name="caseId">Case Id</param>
        /// <returns>json array of document name and url</returns>
        [Route("{caseId:int:min(10000)}")]
        [HttpGet]
        [EnableQuery]   //[EnableQuery(AllowedArithmeticOperators = AllowedArithmeticOperators.None)] //[Queryable]
        public HttpResponseMessage GetDocumentsByCaseId(int caseId)
        {
            try
            {
                List<ArchiveCaseDocumentDetailModel> listResponse = _objArchiveCase.GetDocumentsByCaseIdDocumentId(Convert.ToString(caseId).Trim(), _objArchiveModel);

                if (ReferenceEquals(listResponse, null))
                {
                    HttpResponseMessage resp = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        //Content = new StringContent($"Documents with CaseID = {caseId} does not exists"),
                        Content = new ObjectContent(typeof(string),
                                                    string.Format($"Case with title = {caseId} does not exists"),
                                                    new JsonMediaTypeFormatter()),
                        ReasonPhrase = "Case does not exists"
                    };
                    throw new HttpResponseException(resp);
                    //return listResponse.AsQueryable().DefaultIfEmpty<ArchiveCaseDocumentDetailModel>();
                }
                //return listResponse.AsQueryable();
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, listResponse.AsQueryable());
                return response;
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = ex.Response.StatusCode,
                    Content = ex.Response.Content,
                    ReasonPhrase = ex.Response.ReasonPhrase
                });
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                });
            }
        }

        /// <summary>
        /// Get a single document on basis of DocumentId.
        /// </summary>
        /// <remarks>
        /// Search Case Document on basis of DocumentId.
        /// </remarks>
        /// <param name="documentId">Document Id</param>
        /// <returns>json of document name and url</returns>
        [Route("{documentId:regex(ADOK-([0-9-]+)-([0-9-]+)$)}")] //:minlength(8)
        [HttpGet]
        //[EnableETag]
        public HttpResponseMessage GetDocumentByDocumentId(string documentId)
        {
            try
            {
                List<ArchiveCaseDocumentDetailModel> listResponse = _objArchiveCase.GetDocumentsByCaseIdDocumentId(documentId.Trim(), _objArchiveModel);

                if (ReferenceEquals(listResponse, null))
                {
                    HttpResponseMessage resp = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Content = new StringContent($"Document with ID = {documentId} does not exists"),
                        ReasonPhrase = "Document does not exists"
                    };
                    throw new HttpResponseException(resp);
                }

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, listResponse);
                //response.Headers.Add("Last Modified", result.Modified.ToString("R"));
                //response.Headers.Add("Last Modified",listResponse.);
                response.Headers.ETag = new EntityTagHeaderValue(listResponse[0].ETag);
                return response;
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = ex.Response.StatusCode,
                    Content = ex.Response.Content,
                    ReasonPhrase = ex.Response.ReasonPhrase
                });
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                });
            }
        }

        /// <summary>
        /// Create a single document, in given case folder, Set correct metadata
        /// </summary>
        /// <remarks>
        /// Upload single case document in the "Saksdokumenter" folder.
        /// </remarks>
        /// <param name="caseId">case Id</param>
        /// <param name="fileDataObj">contains file name and file data(combined array buffer)</param>
        /// <returns>Complete json data</returns>
        [Route("{caseId:int:min(10000)}")]
        [HttpPost]
        public HttpResponseMessage UploadSingleDocumentInCaseFolder(int caseId, [FromBody]ArchiveCaseFileModel fileDataObj)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }

                JToken json = JObject.Parse(_objArchiveCase.UploadDocumentInCaseFolder(caseId, fileDataObj, _objArchiveModel));

                if (ReferenceEquals(json, null))
                {
                    HttpResponseMessage resp = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Content = new StringContent($"Document cannot be uploaded in CaseID = {caseId}"),
                        ReasonPhrase = "Document cannot be uploaded"
                    };
                    throw new HttpResponseException(resp);
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, json);
                var routeLink = Url.Link("UpdateCaseMetadata", new { documentId = json["CaseDocumentName"] });
                response.Headers.Location = new Uri(routeLink);
                return response;
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = ex.Response.StatusCode,
                    Content = ex.Response.Content,
                    ReasonPhrase = ex.Response.ReasonPhrase
                });
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                });
            }
        }

        /// <summary>
        /// Batch Upload documents, in given case folder
        /// </summary>
        /// <remarks>
        /// Upload multiple case documents in the "Saksdokumenter" folder.
        /// </remarks>
        /// <param name="caseId">Case Id</param>
        /// <param name="fileDataObj">contains file name and file data(combined array buffer)</param>
        /// <returns>Complete json data</returns>
        [Route("{caseId:int:min(10000)}")]
        [HttpPut]
        public HttpResponseMessage UploadMultipleDocumentInCaseFolder(int caseId, [FromBody]ArchiveCaseFileModel fileDataObj)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }

                JToken json = JObject.Parse(_objArchiveCase.UploadDocumentInCaseFolder(caseId, fileDataObj, _objArchiveModel));

                //if (json == null)
                if (ReferenceEquals(json, null))
                {
                    HttpResponseMessage resp = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Content =
                            new StringContent($"Documents cannot be uploaded in Case with = {caseId}, does not exist"),
                        ReasonPhrase = "Documents cannot be uploaded"
                    };
                    throw new HttpResponseException(resp);
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, json);
                return response;
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = ex.Response.StatusCode,
                    Content = ex.Response.Content,
                    ReasonPhrase = ex.Response.ReasonPhrase
                });
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                });
            }
        }

        /// <summary>
        /// Delete a single document
        /// </summary>
        /// <remarks>
        /// Update the document status in document library on basis of DocumentId.
        /// </remarks>
        /// <param name="documentId">Document id</param>
        /// <returns>Metadata update message</returns>
        [Route("{documentId:regex(ADOK-([0-9-]+)-([0-9-]+)$)}")] //:minlength(8)
        [HttpDelete]
        public HttpResponseMessage DeleteCaseDocument(string documentId)
        {
            try
            {
                // Here we call Regex.Match.
                //Match match = Regex.Match(documentId, @"ADOK-([0-9\-]+)\-([0-9\-]+)$", RegexOptions.IgnoreCase);

                // Here we check the Match instance.
                //if (match.Success)
                //{
                JToken json = JObject.Parse(_objArchiveCase.DeleteCaseDocument(documentId.Trim(), _objArchiveModel));
                HttpResponseMessage response;

                if (!ReferenceEquals(json, null))
                {
                    string responseData = Convert.ToString(json["response"]);
                    if (responseData.ToLower().Equals("Document does not exists".ToLower()))
                    {
                        response = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.NotFound,
                            Content = new StringContent($"Document with Id = {documentId} does not exists"),
                            ReasonPhrase = "Document doesnot exists"
                        };
                        throw new HttpResponseException(response);
                    }

                    //if (Convert.ToString(json["response"]).ToLower().Equals("Wrong Pattern entered for Document ID".ToLower()))
                    //{
                    //    response = new HttpResponseMessage
                    //    {
                    //        StatusCode = HttpStatusCode.NotFound,
                    //        Content = new StringContent($"Document Id : {documentId} is not in correct format"),
                    //        ReasonPhrase = "Document ID is not in correct format"
                    //    };
                    //    throw new HttpResponseException(response);
                    //}

                    //HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NoContent, json);
                    //return Request.CreateResponse(HttpStatusCode.NoContent, json);

                    if (responseData.ToLower().Equals("Case status updated successfully".ToLower()))
                        return Request.CreateResponse(HttpStatusCode.NoContent, json);
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, json);
                }

                response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent($"Document Id : {documentId} is not in correct format"),
                    ReasonPhrase = "Document ID is not in correct format"
                };
                throw new HttpResponseException(response);

                //}
                //else
                //{
                //    HttpResponseMessage resp = new HttpResponseMessage
                //    {
                //        StatusCode = HttpStatusCode.NotFound,
                //        Content = new StringContent($"Document with Id = {documentId} is not in proper format"),
                //        ReasonPhrase = "Document not in proper format"
                //    };
                //    throw new HttpResponseException(resp);
                //}
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = ex.Response.StatusCode,
                    Content = ex.Response.Content,
                    ReasonPhrase = ex.Response.ReasonPhrase
                });
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                });
            }
        }

        /// <summary>
        /// Update a single document
        /// </summary>
        /// <remarks>
        /// Update the metadata of the case document.
        /// </remarks>
        /// <param name="documentId">Document Id</param>
        /// <param name="fileMetadataObj">file metadata object sent from body of caller</param>
        /// <returns></returns>
        [Route("{documentId:regex(ADOK-([0-9-]+)-([0-9-]+)$)}", Name = "UpdateCaseMetadata")] //:minlength(8)
        [HttpPut]
        public HttpResponseMessage UpdateCaseDocumentMetadata(string documentId, [FromBody]ArchiveCaseDocumentModel fileMetadataObj)
        {
            try
            {
                // not required - check again.
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }

                JToken json = (documentId.Length >= 8) ? JObject.Parse(_objArchiveCase.UpdateCaseDocumentMetadata(documentId.Trim(), fileMetadataObj, _objArchiveModel)) : null;

                if (ReferenceEquals(json, null))
                {
                    HttpResponseMessage resp = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Content = new StringContent($"Metadata of DocumentID = {documentId} cannot be updated"),
                        ReasonPhrase = "Metadata of Document cannot be updated"
                    };
                    throw new HttpResponseException(resp);
                }

                //HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NoContent, json);
                if (Convert.ToString(json["response"]).Equals("Case file metadata updated successfully".ToLower()))
                    return Request.CreateResponse(HttpStatusCode.NoContent, json);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, json);
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = ex.Response.StatusCode,
                    Content = ex.Response.Content,
                    ReasonPhrase = ex.Response.ReasonPhrase
                });
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                });
            }
        }

        /// <summary>
        /// To copy documents from case to another
        /// </summary>
        /// <remarks>
        /// To copy the documents from one case to another
        /// </remarks>
        /// <param name="sourceCaseId"></param>
        /// <param name="destinationCaseId"></param>
        /// <param name="copyFlag"></param>
        /// <returns></returns>
        [Route("{sourceCaseId:int:min(10000)}/{destinationCaseId:int:min(10000)}")]
        [HttpPost]
        public HttpResponseMessage CopyCaseDocuments(int sourceCaseId, int destinationCaseId, [FromBody]string copyFlag)
        {
            try
            {
                JToken json = JObject.Parse(_objArchiveCase.CopyCaseDocuments(sourceCaseId, destinationCaseId, _objArchiveModel, copyFlag.Trim()));
                HttpResponseMessage response = null;
                if (!ReferenceEquals(json, null))
                {
                    string responseData = Convert.ToString(json["response"]);
                    if (responseData.ToLower().StartsWith("Library:".ToLower(), StringComparison.Ordinal))
                    {
                        response = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.NotFound,
                            Content = new StringContent(responseData),
                            ReasonPhrase = "Library does not exists"
                        };
                        throw new HttpResponseException(response);
                    }

                    if (responseData.ToLower().Equals("Source Case Folder Does not exists".ToLower()))
                    {
                        response = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.NotFound,
                            Content = new StringContent($"Case with title = {sourceCaseId} does not exists"),
                            ReasonPhrase = "Document cannot be uploaded"
                        };
                        throw new HttpResponseException(response);
                    }

                    if (responseData.ToLower().Equals("Destination Case Folder Does not exists".ToLower()))
                    {
                        response = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.NotFound,
                            Content = new StringContent($"Case with title = {destinationCaseId} does not exists"),
                            ReasonPhrase = "Document cannot be uploaded"
                        };
                        throw new HttpResponseException(response);
                    }

                    if (responseData.ToLower().Equals("Folder Does not exists under Source Case".ToLower()))
                    {
                        response = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.NotFound,
                            Content = new StringContent($"Folder does not exists under CaseID = {sourceCaseId}"),
                            ReasonPhrase = "Document cannot be uploaded"
                        };
                        throw new HttpResponseException(response);
                    }

                    if (responseData.ToLower().Equals("Folder Does not exists under Destination Case".ToLower()))
                    {
                        response = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.NotFound,
                            Content = new StringContent($"Folder does not exists under CaseID = {destinationCaseId}"),
                            ReasonPhrase = "Document cannot be uploaded"
                        };
                        throw new HttpResponseException(response);
                    }
                    if (responseData.ToLower().Contains("No Documents".ToLower()) && Convert.ToString(json["response"]).ToLower().Contains("Case Documents copied successfully".ToLower()))
                    {
                        response = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.NotFound,
                            Content = new StringContent($"Documents copied for one folder and no Documents in another folder under CaseID = {sourceCaseId}"),
                            ReasonPhrase = "No Documents"
                        };
                        throw new HttpResponseException(response);
                    }
                    if (responseData.ToLower().Equals("No Documents".ToLower()))
                    {
                        response = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.NotFound,
                            Content = new StringContent($"There are no Documents in folder under CaseID = {sourceCaseId}"),
                            ReasonPhrase = "No Documents"
                        };
                        throw new HttpResponseException(response);
                    }

                    if (responseData.ToLower().Equals("Case Documents copied successfully".ToLower()))
                    {
                        response = Request.CreateResponse(HttpStatusCode.OK, json);
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.OK, json);
                    }
                }
                return response;
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = ex.Response.StatusCode,
                    Content = ex.Response.Content,
                    ReasonPhrase = ex.Response.ReasonPhrase
                });
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                });
            }
        }

        /// <summary>
        /// Archive case to Record Center
        /// </summary>
        /// <remarks>
        /// Archive case by creating case structure
        /// and moving all case documents into record center
        /// </remarks>
        /// <param name="caseIdToArchive"></param>
        /// <returns></returns>
        [Route("{caseIdToArchive:int:min(10000)}")]
        [HttpPost]
        public HttpResponseMessage ArchiveCase(int caseIdToArchive)
        {
            try
            {
                JToken json = JObject.Parse(_objArchiveCase.CaseIdToArchive(caseIdToArchive));
                HttpResponseMessage response = null;
                if (!ReferenceEquals(json, null))
                {
                    string responseData = Convert.ToString(json["response"]);
                    return response;
                }
                return response;
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = ex.Response.StatusCode,
                    Content = ex.Response.Content,
                    ReasonPhrase = ex.Response.ReasonPhrase
                });
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                });
            }
        }

        #endregion public controller methods
    }
}