using System;
using System.Threading;
using System.Diagnostics;
using System.Windows.Automation;

namespace qBotRunner 
{ 
    class Program 
    { 
        static void Main(string[] args) 
        { 
            if (args.Length == 1 || args.Length == 2)
            {
                Console.WriteLine("Run bot");
                RunBot(args[0], (args.Length == 1) ? "" : args[1]);
            }
            else
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("qBotRunner <automatized_bot_executable_path>");
                Console.WriteLine("qBotRunner <usual_bot_executable_path> <accounts_file_path>");
                Console.WriteLine("Note:");
                Console.WriteLine("Automatized bot version uses fixed accounts file insted of using Open File Dialog");
                Console.WriteLine("Usual bot version can't start on locked computer");
                
            }
        }
        // ----------------------------------------------------------------
        private static void RunBot(string exeFile, string accountsFile)
        {
            try 
            { 
                Process bot = Process.Start(exeFile);
                
                AutomationElement mainWindow = GetMainWindow(bot);

                AutomationElement rootPane = mainWindow.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Pane));
                if (null == rootPane) throw new Exception("Root pane not found");
                
                AutomationElement loadButton = rootPane.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Load accounts list"));
                if (null == loadButton) throw new Exception("Load accounts list button not found");

                AutomationElement startButton = rootPane.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Start all"));
                if (null == startButton) throw new Exception("Start all button not found");

                Console.WriteLine("Click \"Load accounts list\" button");
                ((InvokePattern)loadButton.GetCurrentPattern(InvokePattern.Pattern)).Invoke();
                
                Thread.Sleep(1000);
                
                if (accountsFile != "")
                {
                    // For non-automtized bot version we have to apply accountsFile to OpenDialog
                    ApplyInputFile(mainWindow, accountsFile);
                }
                
                bool stillWaiting = true;
                do
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Waiting for the accounts loading");
                    stillWaiting = !(Boolean)startButton.GetCurrentPropertyValue(AutomationElement.IsEnabledProperty);
                }
                while(stillWaiting);
                
                Console.WriteLine("Click \"Start all\" button");
                ((InvokePattern)startButton.GetCurrentPattern(InvokePattern.Pattern)).Invoke();
                
            } 
            catch (Exception ex) 
            { 
                Console.WriteLine("Fatal: " + ex.Message); 
            } 
        }
        // ----------------------------------------------------------------
        private static AutomationElement GetMainWindow(Process bot) 
        {
            AutomationElement desktop = AutomationElement.RootElement; 
            
            for (int i = 0; i < 5; ++i)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Waiting for the main window");
                
                AutomationElement mainWindow = desktop.FindFirst(TreeScope.Children, new AndCondition(
                    new PropertyCondition(AutomationElement.NameProperty, "MainWindow")
                    , new PropertyCondition(AutomationElement.ProcessIdProperty, bot.Id)
                )); 
                if (null != mainWindow)
                {
                    return mainWindow;
                }
            }
            
            throw new Exception("Main window not found");
        }
        // ----------------------------------------------------------------
        private static void ApplyInputFile (AutomationElement mainWindow, string filename)
        {
            AutomationElement openDialog = mainWindow.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Open"));
            if (null == openDialog) throw new Exception("Open dialog not found");

            AutomationElement fileNameComboBox = openDialog.FindFirst(TreeScope.Children
                , new AndCondition(
                    new PropertyCondition(AutomationElement.NameProperty, "File name:")
                    , new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox)
                )
            );
            if (null == fileNameComboBox) throw new Exception("File name combobox not found");
            
            Console.WriteLine("Apply accounts list file to \"File name\" combobox");
            ((ValuePattern)fileNameComboBox.GetCurrentPattern(ValuePattern.Pattern)).SetValue(filename);

            Thread.Sleep(1000);

            Console.WriteLine("Search for \"Open\" button");
            AutomationElement? openButton = openDialog.FindFirst(TreeScope.Children
                , new AndCondition(
                    new PropertyCondition(AutomationElement.NameProperty, "Open")
                    , new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button)
                )
            );
            
            if (openButton == null) throw new Exception("\"Open\" button not found");

            Console.WriteLine("Click \"Open\" button");
            ((InvokePattern)openButton.GetCurrentPattern(InvokePattern.Pattern)).Invoke();
        }
        // ----------------------------------------------------------------
        private static void PrintChildrenTree (AutomationElement element, string indent = "")
        {
            foreach (AutomationElement child in element.FindAll(TreeScope.Children, Condition.TrueCondition)) 
            {
                string? name = child.GetCurrentPropertyValue(AutomationElement.NameProperty).ToString();
                string type = ((ControlType)child.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty)).ProgrammaticName;
                Console.WriteLine($"{indent}type = \"{type}\", name = \"{name}\"");
                PrintChildrenTree(child, indent + "    ");
            }
        }
        // ----------------------------------------------------------------
        private static void PrintAllPatterns(AutomationElement element)
        {
            Console.WriteLine($"Available patterns of \"{element.GetCurrentPropertyValue(AutomationElement.NameProperty).ToString()}\" element:");
            foreach (AutomationPattern p in element.GetSupportedPatterns())
            {
                Console.WriteLine($"ProgrammaticName: \"{p.ProgrammaticName}\", PatternName: \"{Automation.PatternName(p)}\"");
            }
        }
        // ----------------------------------------------------------------
        private static void PrintChildren(AutomationElement element, string indent = "")
        {
            Condition cond = (indent == "") 
                ? new PropertyCondition(AutomationElement.NameProperty, "Open")
                : Condition.TrueCondition;
            foreach (AutomationElement child in element.FindAll(TreeScope.Children, cond)) 
            {
                string? name = child.GetCurrentPropertyValue(AutomationElement.NameProperty).ToString();
                string type = ((ControlType)child.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty)).ProgrammaticName;
                Console.WriteLine($"{indent}name = \"{name}\", type = \"{type}\"");
                if (name == "Open")
                {
                    PrintChildren(child, indent + "    ");
                }
            }
        }
    }
}