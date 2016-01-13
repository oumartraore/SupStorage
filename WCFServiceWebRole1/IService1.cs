using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WCFServiceWebRole1
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom d'interface "IService1" à la fois dans le code et le fichier de configuration.
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        [WebGet(UriTemplate = "ListRootFolder", ResponseFormat = WebMessageFormat.Json)]
        List<BlobInformation> GetRootFolder();

        [OperationContract]
        [WebGet(UriTemplate = "ListFileFolder?directory={Directory}", ResponseFormat = WebMessageFormat.Json)]
        List<BlobInformation> GetFileFolder(string Directory);

        [OperationContract]
        [WebGet(UriTemplate = "UploadFileFolder?directory={Folder}&chemin={PathFolder}", ResponseFormat = WebMessageFormat.Json)]
        string UploadFileFolder(string Folder, string PathFolder);

        [OperationContract]
        [WebGet(UriTemplate = "ZipDirectory?directory={Folder}", ResponseFormat = WebMessageFormat.Json)]
        string ZipDirectory(string Folder);

        [OperationContract]
        [WebGet(UriTemplate = "DownloadFileAndZip?filePath={FilePath}", ResponseFormat = WebMessageFormat.Json)]
        string DownloadFileAndZip(string FilePath);

    }
}
