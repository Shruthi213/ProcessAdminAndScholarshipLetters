using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

public interface ILetterService
{
    /// <summary>
    /// Combine two letter files into one file.
    /// </summary>
    /// <param name="inputFile1">File path for the first letter.</param>
    /// <param name="inputFile2">File path for the second letter.</param>
    /// <param name="resultFile">File path for the combined letter.</param>
    void CombineTwoLetters(string inputFile1, string inputFile2, string resultFile);
}

public class LetterService : ILetterService
{
    public void CombineTwoLetters(string inputFile1, string inputFile2, string resultFile)
    {
        string content1 = File.ReadAllText(inputFile1);
        string content2 = File.ReadAllText(inputFile2);
        string combinedContent = content1 + content2;
        File.WriteAllText(resultFile, combinedContent);
    }
}

public static class Program
{
    public static void Main()
    {
        #region Define file paths
        string inputFolder = "C:\\Shruthi Akkapelli\\CombineLetters\\Input";
        string archiveFolder = "C:\\Shruthi Akkapelli\\CombineLetters\\Archive";
        string outputFolder = "C:\\Shruthi Akkapelli\\CombineLetters\\Output";
        string reportFileName = "Report.txt";
        #endregion


        // Check if folders exist, create them if not
        CheckAndCreateFolder(archiveFolder);
        CheckAndCreateFolder(outputFolder);

        //Get all directories
        var AdmissionFilePath = $"{inputFolder}\\Admission";
        var ScholorshipFilePath = $"{inputFolder}\\Scholrship";

        var inputAdmissionDirectories = Directory.GetDirectories(AdmissionFilePath, "*").ToList();
        var inputScholorshipDirectories = Directory.GetDirectories(AdmissionFilePath, "*").ToList();

        foreach (var directory in inputAdmissionDirectories)
        {
            var admissionFilesInput = Directory.GetFiles(directory);
            var scholarshipFilesInput = Directory.GetFiles(directory.Replace("Admission", "Scholarship"));

            if (admissionFilesInput.Length == 0 || scholarshipFilesInput.Length == 0)
            {
                Console.WriteLine("No files found in the input folder.");
                Console.ReadLine();
                return;
            }
            var dateCreated = directory.Substring(directory.Length - 8);
            var reportFilePathFinal = Path.Combine($"{outputFolder}\\{dateCreated}", reportFileName);

            CheckAndCreateFolder($"{outputFolder}\\{dateCreated}");


            // Process letters
            ILetterService letterService = new LetterService();
            int combinedLetterCount = 0;
            var studentIDs = new HashSet<string>();

            foreach (string admissionLetterFile in admissionFilesInput)
            {
                string admissionLetterFileName = Path.GetFileNameWithoutExtension(admissionLetterFile);
                string studentID = ExtractStudentID(admissionLetterFileName);

                if (!string.IsNullOrEmpty(studentID))
                {
                    string scholarshipLetterFile = scholarshipFilesInput.FirstOrDefault(f =>
                        ExtractStudentID(Path.GetFileNameWithoutExtension(f)) == studentID);
                    if (scholarshipLetterFile != null)
                    {
                        // Get the archive file paths
                        string admissionLetterArchiveFile = Path.Combine(archiveFolder, Path.GetFileName(admissionLetterFile));
                        string scholarshipLetterArchiveFile = Path.Combine(archiveFolder, Path.GetFileName(scholarshipLetterFile));

                        // Combine letters
                        string combinedLetterFileName = $"{admissionLetterFileName}_Combined.txt";
                        string combinedLetterFilePath = Path.Combine($"{outputFolder}\\{dateCreated}", combinedLetterFileName);

                        letterService.CombineTwoLetters(admissionLetterFile, scholarshipLetterFile, combinedLetterFilePath);
                        combinedLetterCount++;
                        studentIDs.Add(studentID);
                    }
                }
            }


            // Generate report
            string processingDate = DateTime.Now.ToString("MM/dd/yyyy");
            string reportContent = $"Number of Combined Letters: {combinedLetterCount}\n";
            reportContent += "Student IDs:\n";

            foreach (string studentID in studentIDs)
            {
                reportContent += $"{studentID}\n";
            }


            // Check if file already exists. If yes, delete it.     
            if (File.Exists(reportFilePathFinal))
            {
                File.Delete(reportFilePathFinal);
            }

            // Create a new file     
            using (FileStream fs = File.Create(reportFilePathFinal))
            {
                // Add some text to file    
                Byte[] title = new UTF8Encoding(true).GetBytes($"{processingDate} Report\n");
                fs.Write(title, 0, title.Length);

            }

            File.WriteAllText(reportFilePathFinal, reportContent);

                     //Archive all files

                        foreach (var admissions in admissionFilesInput)
                        {
                            var dateFolderName = Path.GetFileName(Path.GetDirectoryName(admissions));
                            ArchiveAfterProcessing(admissions, $"{archiveFolder}\\Admission\\{dateFolderName}", Path.GetFileName(admissions));
               
                        }
                        Directory.Delete(Path.GetDirectoryName(admissionFilesInput.FirstOrDefault()), true);
                        foreach (var scholar in scholarshipFilesInput)
                        {
                            var dateFolderNameScholar = Path.GetFileName(Path.GetDirectoryName(scholar));
                            ArchiveAfterProcessing(scholar, $"{archiveFolder}\\Scholrship\\{dateFolderNameScholar}", Path.GetFileName(scholar));
               
                        }
                        Directory.Delete(Path.GetDirectoryName(scholarshipFilesInput.FirstOrDefault()), true);
        }


    }
    private static void ArchiveAfterProcessing(string inputfilepath, string archivefolder, string filename = "")
    {

        CheckAndCreateFolder(archivefolder);
        string archiveFilePath = Path.Combine(archivefolder, filename);
        File.Move(inputfilepath, archiveFilePath);
    }
    private static void CheckAndCreateFolder(string folder)
    {
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
    }
    private static string ExtractStudentID(string fileName)
    {
        // Use regular expression pattern to extract the student ID
        string pattern = @"\d+";
        Match match = Regex.Match(fileName, pattern);

        if (match.Success)
        {
            return match.Value;
        }

        return string.Empty;
    }
}