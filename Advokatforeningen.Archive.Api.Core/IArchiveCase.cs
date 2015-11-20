using Advokatforeningen.Archive.Api.Model;
using System.Collections.Generic;

namespace Advokatforeningen.Archive.Api.Core
{
    public interface IArchiveCase
    {
        string GetCaseFolderDetails(int caseId, ArchiveCredentialModel objArchiveModel);

        string CreateCaseFolder(int caseId, ArchiveCredentialModel archiveModel);

        string SetPermission(int caseId, string folderName, ArchiveCredentialModel objArchiveModel);

        List<ArchiveCaseDocumentDetailModel> GetDocumentsByCaseIdDocumentId(string caseIdDocumentId, ArchiveCredentialModel archiveModel);

        string UploadDocumentInCaseFolder(int caseId, ArchiveCaseFileModel fileDataObj, ArchiveCredentialModel objArchiveModel);

        string DeleteCaseDocument(string documentId, ArchiveCredentialModel objArchiveModel);

        string UpdateCaseDocumentMetadata(string documentId, ArchiveCaseDocumentModel fileMetadataObj, ArchiveCredentialModel objArchiveModel);

        string CopyCaseDocuments(int sourceCaseId, int destinationCaseId, ArchiveCredentialModel objArchiveModel, string copyFlag);

        string CaseToArchive(int caseIdToArchive, ArchiveCredentialModel objArchiveModel);

        //List<ArchiveCaseDocumentDetailModel> GetDocumentByContradictionDecisionId(string contradictionIdDecisionId, ArchiveCredentialModel objArchiveModel);
    }
}