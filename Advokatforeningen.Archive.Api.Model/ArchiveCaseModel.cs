using System;
using System.ComponentModel.DataAnnotations;

namespace Advokatforeningen.Archive.Api.Model
{
    //public class ArchiveCaseModel
    //{
    //    public string Title { get; set; }
    //    public string CaseNumber { get; set; }
    //    public string CaseSequenceNumber { get; set; }
    //    public string IncidentId { get; set; }
    //    public int Status { get; set; }
    //}

    public class ArchiveCredentialModel
    {
        public string BaseSiteUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        //public string Domain { get; set; }
    }

    public class ArchiveCaseFileModel
    {
        [Required]
        public string FileName { get; set; }

        [Required]
        public string FileData { get; set; }
    }

    public class ArchiveCaseDocumentDetailModel
    {
        [DataType(DataType.DateTime)]
        public DateTime RegisteredDate { get; set; }

        public string Authority { get; set; }
        public string DocumentStatus { get; set; }

        [Range(1000, int.MaxValue)]
        public int CaseNumber { get; set; }

        public string Origin { get; set; }
        public string Category { get; set; }
        public string CategoryOther { get; set; }
        public string Description { get; set; }

        public string ContradictionId { get; set; }

        public string DecisionId { get; set; }
        public string PartyId { get; set; }
        public string OwnerId { get; set; }
        public string CaseDocumentTitle { get; set; }

        [Url]
        public string CaseDocumentUrl { get; set; }

        public string ETag { get; set; }
    }

    public class ArchiveCaseDocumentModel
    {
        public string Authority { get; set; }

        //[Range(1000, int.MaxValue)]
        //public int CaseNumber { get; set; }

        public string Origin { get; set; }
        public string Category { get; set; }

        //public string CategoryOther { get; set; }
        public string ContradictionId { get; set; }

        public string DecisionId { get; set; }
        public string PartyId { get; set; }
        public string OwnerId { get; set; }
        public string DocumentStatus { get; set; }
    }

    public class ItemDetailModel
    {
        [Range(0, int.MaxValue)]
        public int Id { get; set; }

        public string ETag { get; set; }
        public string ListItemEntityTypeFullName { get; set; }
        public string DocumentName { get; set; }
    }
}