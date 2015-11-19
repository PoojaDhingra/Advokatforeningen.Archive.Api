namespace Advokatforeningen.Archive.Api.Repositories.Utilities
{
    public static class Constants
    {
        internal const string KeywordApi = "_api/";

        internal const string DocumentLibraryNo = "Dokumenter";
        internal const string DocumentLibraryEn = "Documents";

        internal const string CaseFolderNameNo = "Saker";
        internal const string CaseFolderNameEn = "Case";

        internal const string DeletedNo = "Slettet";
        internal const string DeletedEn = "Deleted";

        internal const string DocumentStatusDefaultValNo = "Mottatt";
        internal const string DocumentStatusDefaultValEn = "Received";

        //internal const string NemndContributorGroup = "KJ.ND.App.SP.SaksbehandlerDisiplinærnemnd.Contribute";
        //internal const string UtvalgContributorGroup = "KJ.ND.App.SP.SaksbehandlerDisiplinærutvalg.Contribute";
        //internal const string UtvalgOwnerGroup = "KJ.ND.App.SP.SaksbehandlerDisiplinærutvalg.Owner";
        //internal const string LosningGroup = "KJ.ND.App.SP.SaksbehandlerDisiplinærløsning.Contribute";
        //internal const string AllContributeGroup = "Advokatklageordningen-Alle-Bidra";

        internal const string UtvalgContributeGroup = "Advokatklageordningen-Utvalg-Bidra";
        internal const string NemndContributeGroup = "Advokatklageordningen-Nemnd-Bidra";
        internal const string ReadersGroup = "Advokatklageordningen-Alle-Lese";
        internal const string NemndReaderGroup = "Advokatklageordningen-Nemnd-Lese";
        internal const string UtvalgReaderGroup = "Advokatklageordningen-Utvalg-Lese";
        internal const string OwnersGroup = "Advokatklageordningen-Owners";

        //internal const string LibraryName = "Saksdokumenter";
        internal const string NemndResource = "nemnd";

        internal const string UtvalgResource = "utvalg";

        //internal const string DocumentLibraryName = "Dokumenter";
        //internal const string ContributeRoleDefinitionId = "1073741827";

        // template libraries
        internal const string TempUtvalgLib = "Interne Notater Utvalg";

        internal const string TempNemndLib = "Interne Notater Nemnd";
        internal const string TempSaksdokumenter = "Saksdokumenter";
        internal const string TempBeslutning = "Beslutning";
        internal const string TempSladdetBeslutning = "Sladdet beslutning";

        internal const string Fullcontrol = "1073741829";
        internal const string Contribute = "1073741827";
        internal const string Read = "1073741826";

        internal const int NorwegianNoLocaleId = 1044;
        internal const int EnglishUsLocaleId = 1033;

        /*   Header Values  */
        internal const string Accept = "Accept";
        internal const string AcceptHeaderVal = "application/json;odata=verbose";

        internal const string RequestDigest = "X-RequestDigest";
        internal const string ContentType = "Content-Type";
        internal const string BinaryStringRequestBody = "binaryStringRequestBody";

        //internal const string ContradictionId = "ContradictionID";
        //internal const string DecisionId = "DecisionID";
    }
}