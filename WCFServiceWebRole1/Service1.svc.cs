using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WCFServiceWebRole1
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom de classe "Service1" dans le code, le fichier svc et le fichier de configuration.
    // REMARQUE : pour lancer le client test WCF afin de tester ce service, sélectionnez Service1.svc ou Service1.svc.cs dans l'Explorateur de solutions et démarrez le débogage.
    public class Service1 : IService1
    {
        public static List<BlobInformation> blobInformations;
        public static BlobManager blobManager;

        public Service1()
        {
            blobInformations = new List<BlobInformation>();
            blobManager = new BlobManager();
        }

        /* Methode Permettant de lister tous les Dossiers présent dans le Répertoire Root du Container */
        public List<BlobInformation> GetRootFolder()
        {
            var list = blobManager.container.ListBlobs();
            foreach (IListBlobItem item in list)
            {
                var type = item.GetType();
                if (type == typeof(CloudBlobDirectory))
                {
                    string[] name = item.Uri.AbsolutePath.Split('/');

                    BlobInformation blobInformation = new BlobInformation(name[3], item.Uri.AbsolutePath);
                    blobInformations.Add(blobInformation);
                }
            }

            return blobInformations;
        }

        /* Methode Permettant de Lister tous les fichiers dans un Dossier du container donnée en parametre
         * Remarque Si ce Dossier n'existe pas au préalable il sera créer et renvera vide
         */
        public List<BlobInformation> GetFileFolder(string Directory)
        {
            try
            {
                var list = blobManager.container.GetDirectoryReference(Directory).ListBlobs();
                int cpt = 0;
                foreach (IListBlobItem item in list)
                {
                    BlobInformation blobInformation = new BlobInformation(item.Uri.AbsolutePath, item.Uri.AbsolutePath);
                    blobInformations.Add(blobInformation);
                    cpt++;
                }

                if (cpt == 0)
                {
                    blobInformations.Add(new BlobInformation("None", "None"));
                }

            }
            catch (ArgumentNullException)
            {
                Trace.TraceInformation("Parametre de l'URL Incorrect");
            }

            return blobInformations;
        }

        /* Cette Méthode Permet d'uploader un Fichier (.zip) ou (autres format) dans un repertoire spécifique du container */
        public string UploadFileFolder(string Folder, string PathFolder)
        {
            string fileName = Path.GetFileName(PathFolder);
            string extension = Path.GetExtension(PathFolder);

            string messageSortie = "Upload du fichier : ";

            /* Remarque pour Quentin : Le fait de donner n'importe quelle nom au folder de distination
            * dans le container le créera si il n'existe pas.
            */
            CloudBlobDirectory blobDirectory = blobManager.container.GetDirectoryReference(Folder);

            try
            {
                if (extension.Equals(".zip"))
                {
                    using (FileStream fs = new FileStream(PathFolder, FileMode.Open))
                    {
                        using (ZipArchive archive = new ZipArchive(fs))
                        {
                            var entries = archive.Entries;
                            foreach (var entry in entries)
                            {
                                CloudBlockBlob blob = blobDirectory.GetBlockBlobReference(entry.FullName);
                                using (var stream = entry.Open())
                                {
                                    blob.UploadFromStream(stream);
                                }
                            }
                        }
                    }
                }
                else // Cas d'un fichier normal
                {
                    using (var fileStream = System.IO.File.OpenRead(PathFolder))
                    {
                        CloudBlockBlob blockBlob = blobDirectory.GetBlockBlobReference(fileName);
                        blockBlob.UploadFromStream(fileStream);
                    }
                }

            }
            catch (ArgumentException)
            {
                Trace.TraceInformation("Caractere dans le Path Invalid");
                messageSortie = "Caractere dans le Path Invalid Pour le fichier : ";
            }
            catch (FileNotFoundException)
            {
                Trace.TraceInformation("Fichier Introuvable");
                messageSortie = "Fichier Introuvable : ";
            }
            catch (IOException)
            {
                Trace.TraceInformation("Erreur E/S");
                messageSortie = "Erreur E/S pour le fichier : ";
            }

            return messageSortie + fileName;
        }

        /* Méthode qui permet de prendre l'ensemble du contenu d'un dossier sur le disque disque et le 
         * stocker au format .zip dans un dossier archives du container 
         */
        public string ZipDirectory(string Folder)
        {
            string messageSortie = "Compression du Dossier : ";
            FileInfo zipFile = new FileInfo(Folder + ".zip");
            try 
            {
                // On liste tous les fichiers dans le dossier cible du disque
                string[] filePaths = Directory.GetFiles(Folder);

                // On cree un flux à l'aide de Fileinfo
                FileStream fs = zipFile.Create();
                using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Create))
                {
                    foreach (string fileName in filePaths)
                    {
                        zip.CreateEntryFromFile(fileName, Path.GetFileName(fileName), CompressionLevel.Optimal);
                    }

                }

                CloudBlockBlob blob = blobManager.container.GetDirectoryReference("archives").GetBlockBlobReference(zipFile.Name);

                using (FileStream fs2 = zipFile.OpenRead())
                {
                    blob.UploadFromStream(fs2);
                }
            }catch(System.IO.DirectoryNotFoundException)
            {
                messageSortie = "Erreur en spécifiant le Path du fichier : ";
            }
            
            return messageSortie + zipFile.Name;
        }

        /* Methode permettant de Telecharger un fichier (.zip ou autre) du container et le mettre sur votre Bureau */
        public string DownloadFileAndZip(string FilePath)
        {
            string messageSortie = "Téléchargement de : ";
            try
            {
                CloudBlockBlob blockBlob = blobManager.container.GetBlockBlobReference(FilePath);

                // Methode permettant de se positionner sur le Desktop de notre machine
                string pathDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                // On le combine ensuite au nom du fichier, On fait attention de recuperer le nom du fichier dans le Path
                string downloadFile = Path.Combine(pathDesktop, Path.GetFileName(FilePath));

                // Sauvegarde du contenu vers une fichier ouvert en écriture sur le Desktop.
                using (var fileStream = System.IO.File.OpenWrite(downloadFile))
                {
                    blockBlob.DownloadToStream(fileStream);
                }
            }
            catch (Microsoft.WindowsAzure.Storage.StorageException)
            {
                messageSortie = "Erreur lors du Téléchargement de : ";
            }

            return messageSortie + Path.GetFileName(FilePath) + " Sur le Desktop";
        }
    }
}
