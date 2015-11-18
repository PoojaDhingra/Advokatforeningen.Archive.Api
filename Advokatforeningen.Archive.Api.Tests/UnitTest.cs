using Advokatforeningen.Archive.Api.Model;
using Advokatforeningen.Archive.Api.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace Advokatforeningen.Archive.Api.Tests
{
    [TestClass]
    public class UnitTest
    {
        private readonly ArchiveCredentialModel _objArchiveModel;

        #region Constructor

        public UnitTest()
        {
            _objArchiveModel = new ArchiveCredentialModel
            {
                BaseSiteUrl = Convert.ToString(ConfigurationManager.AppSettings["BaseSiteUrl"]),
                Username = Convert.ToString(ConfigurationManager.AppSettings["UserName"]),
                Password = Convert.ToString(ConfigurationManager.AppSettings["Password"])
                //Domain = Convert.ToString(ConfigurationManager.AppSettings["Domain"])
            };
        }

        #endregion Constructor

        #region GetCaseFolderSetails

        [TestMethod]
        public void GetCaseFolderDetails_Int()
        {
            const int caseId = 10000;
            ArchiveCase archiveCaseController = new ArchiveCase();
            archiveCaseController.GetCaseFolderDetails(caseId, _objArchiveModel);
        }

        #endregion GetCaseFolderSetails

        #region CreateCaseFolders

        [TestMethod]
        public void CreateCaseFolder_Int()
        {
            const int caseId = 10000;
            ArchiveCase archiveCaseController = new ArchiveCase();
            archiveCaseController.CreateCaseFolder(caseId, _objArchiveModel);
        }

        #endregion CreateCaseFolders

        #region SetPermissions

        [TestMethod]
        public void SetPermissions_Nemnd_UpperCase()
        {
            const int caseId = 10000;
            const string folderName = "Nemnd";
            ArchiveCase archiveCaseController = new ArchiveCase();
            archiveCaseController.SetPermission(caseId, folderName, _objArchiveModel);
        }

        [TestMethod]
        public void SetPermissions_Nemnd_LowerCase()
        {
            const int caseId = 10000;
            const string folderName = "nemnd";
            ArchiveCase archiveCaseController = new ArchiveCase();
            archiveCaseController.SetPermission(caseId, folderName, _objArchiveModel);
        }

        //this is not executed
        [TestMethod]
        public void SetPermissions_Utvalg_UpperCase()
        {
            const int caseId = 10000;
            const string folderName = "Utvalg";
            ArchiveCase archiveCaseController = new ArchiveCase();
            archiveCaseController.SetPermission(caseId, folderName, _objArchiveModel);
        }

        [TestMethod]
        public void SetPermissions_Utvalg_LowerCase()
        {
            const int caseId = 10000;
            const string folderName = "utvalg";
            ArchiveCase archiveCaseController = new ArchiveCase();
            archiveCaseController.SetPermission(caseId, folderName, _objArchiveModel);
        }

        [TestMethod]
        public void SetPermissions_Utvalg_WrongEntry()
        {
            const int caseId = 10000;
            const string folderName = "UtvalgA";
            ArchiveCase archiveCaseController = new ArchiveCase();
            var result = JObject.Parse(archiveCaseController.SetPermission(caseId, folderName, _objArchiveModel));
            Assert.AreEqual("Wrong Authority name entered", result["response"]);
        }

        [TestMethod]
        public void SetPermissions_Nemnd_WrongEntry()
        {
            const int caseId = 10000;
            const string folderName = "NemndA";
            ArchiveCase archiveCaseController = new ArchiveCase();
            var result = JObject.Parse(archiveCaseController.SetPermission(caseId, folderName, _objArchiveModel));
            Assert.AreEqual("Wrong Authority name entered", result["response"]);
        }

        #endregion SetPermissions

        #region DeleteDocument

        [TestMethod]
        public void DeleteCaseDocument_DocId_UpperCase()
        {
            const string documentId = "ADOK-1-36";
            ArchiveCase archiveCaseController = new ArchiveCase();
            archiveCaseController.DeleteCaseDocument(documentId, _objArchiveModel);
        }

        [TestMethod]
        public void DeleteCaseDocument_DocId_LowerCase()
        {
            const string documentId = "adok-1-36";
            ArchiveCase archiveCaseController = new ArchiveCase();
            archiveCaseController.DeleteCaseDocument(documentId, _objArchiveModel);
        }

        [TestMethod]
        public void DeleteCaseDocument_DocId_LessLength()
        {
            string documentId = "ADOK";
            ArchiveCase archiveCaseController = new ArchiveCase();
            archiveCaseController.DeleteCaseDocument(documentId, _objArchiveModel);
        }

        [TestMethod]
        public void DeleteCaseDocument_DocId_WrongPattern()
        {
            const string documentId = "testtest";
            ArchiveCase archiveCaseController = new ArchiveCase();
            archiveCaseController.DeleteCaseDocument(documentId, _objArchiveModel);
        }

        #endregion DeleteDocument

        #region UploadSingleDocument

        [TestMethod]
        public void UploadSingleDocument_Word()
        {
            const int caseId = 10000;
            ArchiveCase archiveCaseController = new ArchiveCase();
            ArchiveCaseFileModel fileDataObj = new ArchiveCaseFileModel
            {
                FileName = "word.docx",
                FileData = "Sample Docs Sample Docs Sample Docs Sample Docs Sample Docs Sample Docs Sample Docs"
            };
            archiveCaseController.UploadDocumentInCaseFolder(caseId, fileDataObj, _objArchiveModel);
        }

        [TestMethod]
        public void UploadSingleDocument_Notepad()
        {
            const int caseId = 10000;
            ArchiveCase archiveCaseController = new ArchiveCase();
            ArchiveCaseFileModel fileDataObj = new ArchiveCaseFileModel
            {
                FileName = "Notepad.txt",
                FileData = "Notepad text Sample Docs Sample Docs Sample Docs Sample Docs Sample Docs Sample Docs Sample Docs"
            };
            archiveCaseController.UploadDocumentInCaseFolder(caseId, fileDataObj, _objArchiveModel);
        }

        [TestMethod]
        public void UploadSingleDocument_PPT()
        {
            const int caseId = 10000;
            ArchiveCase archiveCaseController = new ArchiveCase();
            ArchiveCaseFileModel fileDataObj = new ArchiveCaseFileModel
            {
                FileName = "Powerpoint.pptx",//6.xlsx
                FileData = "Powerpoint text Sample Docs Sample Docs Sample Docs Sample Docs Sample Docs Sample Docs Sample Docs"
            };
            archiveCaseController.UploadDocumentInCaseFolder(caseId, fileDataObj, _objArchiveModel);
        }

        [TestMethod]
        public void UploadSingleDocument_Excel()
        {
            const int caseId = 10000;
            ArchiveCase archiveCaseController = new ArchiveCase();
            ArchiveCaseFileModel fileDataObj = new ArchiveCaseFileModel
            {
                FileName = "Excel.xlsx",
                FileData = "Excel text Sample Docs Sample Docs Sample Docs Sample Docs Sample Docs Sample Docs Sample Docs"
            };
            archiveCaseController.UploadDocumentInCaseFolder(caseId, fileDataObj, _objArchiveModel);
        }

        [TestMethod]
        public void UploadSingleDocument_Image()
        {
            const int caseId = 10000;
            ArchiveCase archiveCaseController = new ArchiveCase();
            ArchiveCaseFileModel fileDataObj = new ArchiveCaseFileModel
            {
                FileName = "Desert.jpg",
                FileData = "Excel text Sample Docs Sample Docs Sample Docs Sample Docs Sample Docs Sample Docs Sample Docs"
            };
            archiveCaseController.UploadDocumentInCaseFolder(caseId, fileDataObj, _objArchiveModel);
        }

        [TestMethod]
        public void UploadSingleDocument_Exe()
        {
            const int caseId = 10000;
            ArchiveCase archiveCaseController = new ArchiveCase();
            ArchiveCaseFileModel fileDataObj = new ArchiveCaseFileModel
            {
                FileName = "fiddler4setup.exe",
                FileData = "Sample Docs Sample Docs Sample Docs Sample Docs Sample Docs Sample Docs Sample Docs"
            };
            string extension = Path.GetExtension(fileDataObj.FileName);
            var result = JObject.Parse(archiveCaseController.UploadDocumentInCaseFolder(caseId, fileDataObj, _objArchiveModel));
            Assert.AreEqual("Invalid file Extension " + extension, result["response"]);
        }

        [TestMethod]
        public void UploadSingleDocument_Json()
        {
            const int caseId = 10000;
            ArchiveCase archiveCaseController = new ArchiveCase();
            ArchiveCaseFileModel fileDataObj = new ArchiveCaseFileModel
            {
                FileName = "fiddler4setup.json",
                FileData = "{ 'employees':[" +
                           "{ 'firstName':'Peter', 'lastName':'Jones'}" +
                           "]}"
            };
            string extension = Path.GetExtension(fileDataObj.FileName);
            var result = JObject.Parse(archiveCaseController.UploadDocumentInCaseFolder(caseId, fileDataObj, _objArchiveModel));
            Assert.AreEqual("Invalid file Extension " + extension, result["response"]);
        }

        [TestMethod]
        public void UploadSingleDocument_JavaScript()
        {
            const int caseId = 10000;
            ArchiveCase archiveCaseController = new ArchiveCase();
            ArchiveCaseFileModel fileDataObj = new ArchiveCaseFileModel
            {
                FileName = "script.js",
                FileData = "<script>" +
                           "alert('Javascript File');" +
                           "</script>"
            };
            string extension = Path.GetExtension(fileDataObj.FileName);
            var result = JObject.Parse(archiveCaseController.UploadDocumentInCaseFolder(caseId, fileDataObj, _objArchiveModel));
            Assert.AreEqual("Invalid file Extension " + extension, result["response"]);
        }

        #endregion UploadSingleDocument

        #region UploadMultipleDocument

        [TestMethod]
        public void UploadMultipleDocument_Valid()
        {
            const int caseId = 10000;
            ArchiveCase archiveCaseController = new ArchiveCase();
            List<ArchiveCaseFileModel> multipleDocs = new List<ArchiveCaseFileModel>
            {
                new ArchiveCaseFileModel {FileName = "A.xlsx", FileData = "Sample Docs Text 1"},
                new ArchiveCaseFileModel {FileName = "B.xls", FileData = "Sample Docs Text 2"},
                new ArchiveCaseFileModel {FileName = "C.jpg", FileData = "Sample Docs Text 3"},
                new ArchiveCaseFileModel {FileName = "D.jpeg", FileData = "Sample Docs Text 4"},
                new ArchiveCaseFileModel {FileName = "E.gif", FileData = "Sample Docs Text 5"},
                new ArchiveCaseFileModel {FileName = "F.png", FileData = "Sample Docs Text 6"},
                new ArchiveCaseFileModel {FileName = "F.doc", FileData = "Sample Docs Text 7"},
                new ArchiveCaseFileModel {FileName = "H.docx", FileData = "Sample Docs Text 8"},
                new ArchiveCaseFileModel {FileName = "I.pdf", FileData = "Sample Docs Text 9"},
                new ArchiveCaseFileModel {FileName = "J.pptx", FileData = "Sample Docs Text 10"},
                new ArchiveCaseFileModel {FileName = "K.ppt", FileData = "Sample Docs Text 11"},
                new ArchiveCaseFileModel {FileName = "L.txt", FileData = "Sample Docs Text 12"}
            };

            foreach (var fileDataObj in multipleDocs)
            {
                archiveCaseController.UploadDocumentInCaseFolder(caseId, fileDataObj, _objArchiveModel);
            }
        }

        [TestMethod]
        public void UploadMultipleDocument_InValid()
        {
            const int caseId = 10000;
            ArchiveCase archiveCaseController = new ArchiveCase();
            List<ArchiveCaseFileModel> multipleDocs = new List<ArchiveCaseFileModel>
            {
                new ArchiveCaseFileModel {FileName = "C.exe", FileData = "Sample Docs Text 3"},
                new ArchiveCaseFileModel {FileName = "D.json", FileData = "Sample Docs Text 4"}
            };

            foreach (var fileDataObj in multipleDocs)
            {
                string extension = Path.GetExtension(fileDataObj.FileName);
                var result = JObject.Parse(archiveCaseController.UploadDocumentInCaseFolder(caseId, fileDataObj, _objArchiveModel));
                Assert.AreEqual("Invalid file Extension " + extension, result["response"]);
            }
        }

        [TestMethod]
        public void UploadMultipleDocument_MixValidInvalid()
        {
            const int caseId = 10000;
            ArchiveCase archiveCaseController = new ArchiveCase();
            List<ArchiveCaseFileModel> multipleDocs = new List<ArchiveCaseFileModel>
            {
                new ArchiveCaseFileModel {FileName = "A.docx", FileData = "Sample Docs Text 1"},
                new ArchiveCaseFileModel {FileName = "B.pptx", FileData = "Sample Docs Text 2"},
                new ArchiveCaseFileModel {FileName = "C.exe", FileData = "Sample Docs Text 3"},
                new ArchiveCaseFileModel {FileName = "D.json", FileData = "Sample Docs Text 4"}
            };

            foreach (var fileDataObj in multipleDocs)
            {
                string extension = Path.GetExtension(fileDataObj.FileName);
                var result = JObject.Parse(archiveCaseController.UploadDocumentInCaseFolder(caseId, fileDataObj, _objArchiveModel));
                string testResult = Convert.ToString(result["response"]);
                if (testResult.Contains("Invalid file Extension"))
                {
                    Assert.AreEqual("Invalid file Extension " + extension, result["response"]);
                }
            }
        }

        #endregion UploadMultipleDocument

        #region UpdateMetadata

        [TestMethod]
        public void UpdateMetadata_FullData()
        {
            string documentId = "ADOK-1-250";
            ArchiveCase archiveCaseController = new ArchiveCase();
            ArchiveCaseDocumentModel objArchiveCaseDocumentModel = new ArchiveCaseDocumentModel
            {
                Authority = "Utvalg",
                Category = "Dom",
                ContradictionId = "d571d19a-8242-4608-8186-6799627865ba",
                DecisionId = "d571d19a-8242-4608-8186-6799627865ba",
                DocumentStatus = "Mottatt",
                Origin = "Utvalg",
                OwnerId = "12",
                PartyId = "23"
            };

            archiveCaseController.UpdateCaseDocumentMetadata(documentId, objArchiveCaseDocumentModel, _objArchiveModel);
        }

        [TestMethod]
        public void UpdateMetadata_PartialData()
        {
            string documentId = "ADOK-1-250";
            ArchiveCase archiveCaseController = new ArchiveCase();
            ArchiveCaseDocumentModel objArchiveCaseDocumentModel = new ArchiveCaseDocumentModel
            {
                Authority = "Utvalg",
                Category = "Dom",
                ContradictionId = "d571d19a-8242-4608-8186-6799627865ba",
                DecisionId = "",
                DocumentStatus = "",
                Origin = "Utvalg",
                OwnerId = "",
                PartyId = ""
            };

            archiveCaseController.UpdateCaseDocumentMetadata(documentId, objArchiveCaseDocumentModel, _objArchiveModel);
        }

        [TestMethod]
        public void UpdateMetadata_WrongDocumentId()
        {
            string documentId = "ADOK-2-250";
            ArchiveCase archiveCaseController = new ArchiveCase();
            ArchiveCaseDocumentModel objArchiveCaseDocumentModel = new ArchiveCaseDocumentModel
            {
                Authority = "Utvalg",
                Category = "Dom",
                ContradictionId = "d571d19a-8242-4608-8186-6799627865ba",
                DecisionId = "",
                DocumentStatus = "",
                Origin = "Utvalg",
                OwnerId = "",
                PartyId = ""
            };

            var result = JObject.Parse(archiveCaseController.UpdateCaseDocumentMetadata(documentId, objArchiveCaseDocumentModel, _objArchiveModel));
            Assert.AreEqual("Document ID not found.", result["response"]);
        }

        [TestMethod]
        public void UpdateMetadata_NoData()
        {
            string documentId = "ADOK-1-1264";
            ArchiveCase archiveCaseController = new ArchiveCase();

            var result = JObject.Parse(archiveCaseController.UpdateCaseDocumentMetadata(documentId, null, _objArchiveModel));
            Assert.AreEqual("No Metadata found to update for " + documentId + ".", result["response"]);
        }

        [TestMethod]
        public void UpdateMetadata_NoDocumentId()
        {
            string documentId = string.Empty;
            ArchiveCase archiveCaseController = new ArchiveCase();
            ArchiveCaseDocumentModel objArchiveCaseDocumentModel = new ArchiveCaseDocumentModel
            {
                Authority = "Utvalg",
                Category = "Dom",
                ContradictionId = "d571d19a-8242-4608-8186-6799627865ba",
                DecisionId = "",
                DocumentStatus = "",
                Origin = "Utvalg",
                OwnerId = "",
                PartyId = ""
            };
            var result = JObject.Parse(archiveCaseController.UpdateCaseDocumentMetadata(documentId, objArchiveCaseDocumentModel, _objArchiveModel));
            Assert.AreEqual("Document ID not found.", result["response"]);
        }

        #endregion UpdateMetadata

        #region CopyDocuments

        //[TestMethod]
        //public void CopyFiles_Source_Dest()
        //{
        //    const int sourceCaseId = 10000;
        //    const int destCaseId = 11000;
        //    ArchiveCase archiveCaseController = new ArchiveCase();
        //    var result = JObject.Parse(archiveCaseController.CopyCaseDocuments(sourceCaseId, destCaseId, _objArchiveModel));
        //    Assert.AreEqual("Case Documents copied successfully", result["response"]);
        //}

        //[TestMethod]
        //public void CopyFiles_WrongSource_Dest()
        //{
        //    const int sourceCaseId = 122222;
        //    const int destCaseId = 11000;
        //    ArchiveCase archiveCaseController = new ArchiveCase();
        //    var result = JObject.Parse(archiveCaseController.CopyCaseDocuments(sourceCaseId, destCaseId, _objArchiveModel));
        //    Assert.AreEqual("Source Case Folder Does not exists", result["response"]);
        //}

        //[TestMethod]
        //public void CopyFiles_Source_WrongDest()
        //{
        //    const int sourceCaseId = 10000;
        //    const int destCaseId = 11000123;
        //    ArchiveCase archiveCaseController = new ArchiveCase();
        //    var result = JObject.Parse(archiveCaseController.CopyCaseDocuments(sourceCaseId, destCaseId, _objArchiveModel));
        //    Assert.AreEqual("Destination Case Folder Does not exists", result["response"]);
        //}

        //[TestMethod]
        //public void CopyFiles_NoDocumentsAtSource()
        //{
        //    const int sourceCaseId = 13000;
        //    const int destCaseId = 11000;
        //    ArchiveCase archiveCaseController = new ArchiveCase();
        //    var result = JObject.Parse(archiveCaseController.CopyCaseDocuments(sourceCaseId, destCaseId, _objArchiveModel));
        //    Assert.AreEqual("No Documents", result["response"]);
        //}

        //[TestMethod]
        //public void CopyFiles_NoDocumentsInFirstFolder()
        //{
        //    const int sourceCaseId = 15000;
        //    const int destCaseId = 11000;
        //    ArchiveCase archiveCaseController = new ArchiveCase();
        //    var result = JObject.Parse(archiveCaseController.CopyCaseDocuments(sourceCaseId, destCaseId, _objArchiveModel));
        //    Assert.AreEqual("Case Documents copied successfullyNo Documents", result["response"]);
        //}

        //[TestMethod]
        //public void CopyFiles_NoDocumentsInSecondFolder()
        //{
        //    const int sourceCaseId = 14000;
        //    const int destCaseId = 11000;
        //    ArchiveCase archiveCaseController = new ArchiveCase();
        //    var result = JObject.Parse(archiveCaseController.CopyCaseDocuments(sourceCaseId, destCaseId, _objArchiveModel));
        //    Assert.AreEqual("No DocumentsCase Documents copied successfully", result["response"]);
        //}

        //[TestMethod]
        //public void CopyFiles_OverriteDocuments()
        //{
        //    const int sourceCaseId = 10000;
        //    const int destCaseId = 11000;
        //    ArchiveCase archiveCaseController = new ArchiveCase();
        //    var result = JObject.Parse(archiveCaseController.CopyCaseDocuments(sourceCaseId, destCaseId, _objArchiveModel));
        //    Assert.AreEqual("Case Documents copied successfully", result["response"]);
        //}

        //[TestMethod]
        //public void CopyFiles_NoSourceFolder()
        //{
        //    const int sourceCaseId = 12000;
        //    const int destCaseId = 10000;
        //    ArchiveCase archiveCaseController = new ArchiveCase();
        //    var result = JObject.Parse(archiveCaseController.CopyCaseDocuments(sourceCaseId, destCaseId, _objArchiveModel));
        //    Assert.AreEqual("Folder Does not exists under Source Case", result["response"]);
        //}

        ////[TestMethod]
        ////public void CopyFiles_NoDestinationFolder()
        ////{
        ////    const int sourceCaseId = 10000;
        ////    const int destCaseId = 12000;
        ////    ArchiveCase archiveCaseController = new ArchiveCase();
        ////    var result = JObject.Parse(archiveCaseController.CopyCaseDocuments(sourceCaseId, destCaseId, _objArchiveModel));
        ////    Assert.AreEqual("Folder Does not exists under Destination Case", result["response"]);
        ////}

        #endregion CopyDocuments
    }
}