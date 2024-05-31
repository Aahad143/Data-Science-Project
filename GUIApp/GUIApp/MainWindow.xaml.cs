using System;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using Python.Runtime;
using System.Windows.Controls;

namespace GUIApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Print the scriptPath to the terminal
            Console.WriteLine("Script Path: ");

            RunPythonScript("test1", "hello_world", new dynamic[] { "Hello from C#!", 123, true });
        }
        private void RunPythonScript(string scriptName, string functionName, dynamic[] args)
        {
            try
            {
                string pythonPath = Environment.GetEnvironmentVariable("PYTHONPATH");
                if (string.IsNullOrEmpty(pythonPath))
                {
                    pythonPath = Environment.GetEnvironmentVariable("PATH");
                }

                // Search for python311.dll within the path(s)
                string dllPath = FindPythonDll(pythonPath);

                if (!string.IsNullOrEmpty(dllPath))
                {
                    Runtime.PythonDLL = dllPath;
                }

                /*Runtime.PythonDLL = @"C:\Users\aahad\AppData\Local\Programs\Python\Python311\python311.dll";*/

                // Initialize the Python engine
                PythonEngine.Initialize();

                using (Py.GIL())
                {
                    // Add the directory containing nn.py to the Python path
                    dynamic sys = Py.Import("sys");

                    // Get the base directory of the application
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                    string[] baseDirSplit = baseDirectory.Split("\\");

                    int datascprojIndex = Array.IndexOf(baseDirSplit, "Data-Science-Project") + 1;

                    // Join the elements using backslash as separator
                    string joinedPath = string.Join("\\", baseDirSplit.Take(datascprojIndex).ToArray());

                    // Define the relative path to the Python script
                    string scriptDirectory = Path.Combine(joinedPath, "py_scripts");

                    // Normalize the path to remove any ".." or "." components
                    scriptDirectory = Path.GetFullPath(scriptDirectory);

                    sys.path.append(scriptDirectory);

                    // Import the Python script
                    dynamic script = Py.Import(scriptName);

                    // Get the function from the script
                    dynamic function = script.GetAttr(functionName);

                    // Prepare arguments
                    PyObject[] pyArgs = new PyObject[args.Length];
                    for (int i = 0; i < args.Length; i++)
                    {
                        pyArgs[i] = new PyString(args[i].ToString());
                    }

                    // Convert array to tuple
                    PyTuple pyTuple = new PyTuple(pyArgs);

                    // Call the function with arguments
                    PyObject result = function.Invoke(pyTuple);

                    // Convert result to string and display it
                    string resultString = result.As<string>();

                    // Display the result
/*                    MessageBox.Show(resultString, "Python Function Result", MessageBoxButton.OK, MessageBoxImage.Information);
*/

                    // Access the TextBlock using its name
                    TextBlock textBlock = (TextBlock)FindName("hello");

                    // Modify the Text attribute
                    textBlock.Text = resultString;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Shutdown the Python engine
                PythonEngine.Shutdown();
            }
        }
        string FindPythonDll(string searchPaths)
        {
            if (string.IsNullOrEmpty(searchPaths))
            {
                return null; // No paths provided
            }

            // Split the search paths into individual paths (assuming separated by semicolons)
            string[] paths = searchPaths.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            // Search for the DLL in each path
            foreach (string path in paths)
            {
                string potentialDllPath = Path.Combine(path, @"python311.dll"); // Change @"python311.dll" to @"python<your-py-version>.dll"
                if (File.Exists(potentialDllPath))
                {
                    return potentialDllPath; // Found the DLL
                }
            }

            // DLL not found in any of the search paths
            return null;
        }
    }
}