/*
 * RockyTextCreator
 * 
 * John Welter
 * 2016
 * 
 * visual program to aid in creation of NES text files
 * 
 * keeps a data base of text data blocks, which can be converted into readable chunks of data by an NES assembler
 * can also save projects as XML files to be converted later
 * 
 */


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.IO;

namespace RockyNESTextTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {

        private String flname = "";
        private txtData currentData = new txtData(); 
        private int currentDataIndex = 0;
        private ObservableCollection<txtData> currentFile = new ObservableCollection<txtData>();
        private String charTable = "0123456789abcdefghijklmnopqrstuvwxyz.?!,:'\"%~$-*"; // all acceptable characters
        private bool extraDataCheck = false;
        private txtData dummyTxt = new txtData(); // creates dummy text to use as a buffer when starting/ deleting

        //conversion values
        bool parseTest;
        String output;
        String tab;
        char ptr;
        int counter;
        int chars;
        int parse;

       //////////////////////////////////////////////////////////////////////////////////////////////////////

        public MainWindow()
        {

            InitializeComponent();
            listView.ItemsSource = currentFile;
            textBox.Text = currentData.text;
            textBox1.Text = currentData.label;
            dummyTxt.label = "";
            dummyTxt.text = "";

        }

        private void text_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox TextDataText = (TextBox)sender;
            currentData.text = TextDataText.Text; //sets text of current data to what's in the text box
        }

        private void label_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox LabelText = (TextBox)sender; 
            currentData.label = LabelText.Text; //sets label of current data to what's in the label box
        }
        
        
        private void TextValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^-0-9a-z.?!,:'\"%~$-*RNLWPS]+"); // allows only acceptable input to text box
            e.Handled = regex.IsMatch(e.Text);
        }
        private void LabelValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^-0-9a-zA-Z_]+"); // allows only acceptable input to label box
            e.Handled = regex.IsMatch(e.Text);
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            ListView L = (ListView)sender;          
            currentDataIndex = L.SelectedIndex;     //changes label and text box to link to selected data
            currentData = (txtData)L.SelectedItem;
            if(currentData != null)
            {
                textBox.Text = currentData.text;
                textBox1.Text = currentData.label;
            }
            
        }

       

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Button clicked = (Button)sender;
            if (clicked.Name.Equals("New"))
            {
                //creates new file. does not automatically save changes.
                
                //Console.WriteLine("Make new file/nask before continuing/nclose old file, open new");
                MessageBoxResult confirmOpen = MessageBox.Show("Starting a new file will close the current file. Any unsaved changes will be lost.", "Confirmation", MessageBoxButton.OKCancel);

                if (confirmOpen == MessageBoxResult.OK)
                {
                    currentFile.Clear();
                    currentData = dummyTxt;
                    textBox.Text = "";
                    textBox1.Text = "";
                    flname = "";
                }
               
            }

            else if (clicked.Name.Equals("Open"))
            {
               
                //opens file from XML. does not automatically save changes.


                //Console.WriteLine("Open another file/nask before continuing/nclose old file, open new");
                MessageBoxResult confirmOpen = MessageBox.Show("Opening another file will close the current file. Any unsaved changes will be lost.", "Confirmation", MessageBoxButton.OKCancel);
            
                if(confirmOpen == MessageBoxResult.OK)
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "XML (*.xml)|*.xml";
                    if (openFileDialog.ShowDialog() == true)
                    {
                        currentFile.Clear();
                        System.Xml.Serialization.XmlSerializer reader =
                            new System.Xml.Serialization.XmlSerializer(typeof(ObservableCollection<txtData>));
                        System.IO.StreamReader file = new System.IO.StreamReader(openFileDialog.FileName);
                        ObservableCollection<txtData> inFile = (ObservableCollection<txtData>)reader.Deserialize(file);
                        foreach (txtData TD in inFile)
                        {
                            currentFile.Add(TD);
                        }

                        flname = openFileDialog.SafeFileName;
                        file.Close();
                    }


                }
            
            }
            else if (clicked.Name.Equals("Save"))
            {

                //saves text file as XML

                //Console.WriteLine("Save current file");
                //SaveFileDialog saveFileDialog = new SaveFileDialog();
                //saveFileDialog.Filter = "Rocky NES Text (*.rnt)|*.rnt";
                //if (saveFileDialog.ShowDialog() == true)
                //{
                //   String allTxt = "";
                //    foreach(txtData txt in currentFile)
                //    {
                //        allTxt += txt.label + "\n";
                //        allTxt += txt.text + "\n";
                //    }
                //    File.WriteAllText(saveFileDialog.FileName, allTxt);
                //}

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = flname;
                saveFileDialog.Filter = "XML (*.xml)|*.xml";
                if (saveFileDialog.ShowDialog() == true)
                {
                    System.Xml.Serialization.XmlSerializer writer =
                      new System.Xml.Serialization.XmlSerializer(typeof(ObservableCollection<txtData>));

                    System.IO.FileStream file = System.IO.File.Create(saveFileDialog.FileName);

                    writer.Serialize(file, currentFile);
                    file.Close();

                }


            }

            else if (clicked.Name.Equals("Export"))
            {

                //exports NES readable file by converting each text data into a block of bytes

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "NES data file (*.i)|*.i|ASM file (*.asm)|*.asm";
                if (saveFileDialog.ShowDialog() == true)
                {
                    String allTxt = "";
                    foreach (txtData txt in currentFile)
                    {
                        allTxt += txt.label + ":\n";
                        try
                        {
                            allTxt += convert(txt.text);
                        }
                        catch (System.Exception valueError)
                        {
                            MessageBoxResult errorParsing = MessageBox.Show(valueError.Message, "Value Error", MessageBoxButton.OK);
                            return;
                        }

                        allTxt += "\n\n";
                        
                    }
                    File.WriteAllText(saveFileDialog.FileName, allTxt);
                }

            }
            else if(clicked.Name.Equals("Add"))
            {

                //creates new text data and adds it to the file

                txtData TD = new txtData();
                TD.label = "NewData";
                TD.text = "newtext";
                currentFile.Add(TD);
                listView.SelectedIndex = currentFile.Count-1;

                //Console.WriteLine("Add new text data");
            }
            else if(clicked.Name.Equals("Delete"))
            {
                //removes selected data from file 

                if (listView.SelectedIndex >= 0)
                {
                    currentFile.RemoveAt(listView.SelectedIndex);
                    currentData = dummyTxt;
                    textBox.Text = "";
                    textBox1.Text = "";

                }

            }
            else if (clicked.Name.Equals("Info"))
            {
                MessageBox.Show("Rocky NES Text creator\nFor use with the Rocky NES text engine\nVersion 1.1\nCopyright 2016 John Welter\nContact: johnwelter@me.com", "Info");

            }
        }
        /* better extra data check - in production
        private void extraData(int amount, int sections, char code)
        {
            if(amount%sections != 0)
            {

                throw new System.Exception("make sure value size is correct after code: " + code);

            }
            String[] tempOut = new String[sections];

            for(int i = 0; i < sections, i++)
            {
                for(int j = 0; j < amount; j++)
                {
                    counter++;
                    tempOut[j] += input[counter];
                }
                Regex regex = new Regex("[^0-9]+");
                parseTest = regex.IsMatch(tempOut);

                if (parseTest)
                {

                    //extraDataCheck = false;
                    throw new System.Exception("error in parsing for code: " + code + ". check to see if all additive codes have appropriate values.", null);

                }
                else
                {
                    int tempInt = int.Parse(tempOut);
                    tempInt = tempInt % 256;
                    output += "$" + tempInt.ToString("X2");

                    //extraDataCheck = false;

                }
           }

                //Console.WriteLine(tempOut);
        }
        */
        private void resetConvertValues()
        {
            parseTest = false;
            output = "";
            tab = "  .db ";
            ptr = '0';
            counter = 0;
            chars = 0;
            parse = 0;
            output += tab;
        }
        private String convert(String input)
        {
            resetConvertValues();

            // INPUT - text of current text data to be converted
            //
            //converts a single text data to the format:
            //Label:
            //tab, .db, hex data (up to 16 per line), break

            //IE
            //label:    Butts
            //text :    butts are cool, bruh dude
            //
            //Butts:
            //  .db $0B, $1E, $1D, $1D, $1C, $FA, $0A, $1B, $0C, $18, $18, $15, $27, $FA, $0B, $1B
            //  .db $1E, $11, $FA, $0D, $1E, $0D, $0E, $FF


            while(counter < input.Length)
            {
                //Console.WriteLine(counter.ToString());
                ptr = input[counter];   // gets character at the input
                //Console.WriteLine(ptr);
                parse = charTable.IndexOf(ptr); // gets the value of the character from the character table
                if(!extraDataCheck && parse != -1)
                {   
                    //if there's no additioanl data to check for and parse is not a command, add it to the output
                    output += "$" + parse.ToString("X2");
                }
                else
                {
                    //if there's extra data to parse, gather the next three characters, and convert them into one Hex value
                    if(extraDataCheck)
                    {
                        String tempOut = "";
                        tempOut += ptr;
                        counter++;
                        tempOut += input[counter];
                        counter++;
                        tempOut += input[counter];

                        //Console.WriteLine(tempOut);

                        Regex regex = new Regex("[^0-9]+");
                        parseTest = regex.IsMatch(tempOut);

                        if (parseTest)
                        {

                            extraDataCheck = false;
                            throw new System.Exception("error in parsing. check to see if all additive codes have appropriate values.", null);

                        }
                        else
                        {
                            int tempInt = int.Parse(tempOut);
                            tempInt = tempInt % 256;
                            output += "$" + tempInt.ToString("X2");
                            extraDataCheck = false;
                            
                        }

                    }
                    /*
                    else if(ptr == 'Q')
                    {
                        

                    }
                    */
                    else if(ptr == 'P')
                    {
                        //pause for x frames
                        output += "$F8";
                        extraDataCheck = true;
                        //try(extraData(3, 1))
                        //{
                        //do something? I dunno
                        //}
                        //catch(System.Exception parseException)
                        //{throw new System.Exception valueException(parseException.message);
                        //}
                    }
                    else if (ptr == 'S')
                    {
                        //change speed to x
                        output += "$F9";
                        extraDataCheck = true;
                        //extraData(3, 1)
                        
                    }
                    else if (ptr == 'W')
                    {
                        //Input Wait
                        output += "$FE";
                    }
                    else if (ptr == 'L')
                    {
                        //Loop from beggining
                        output += "$FD";
                    }
                    else if (ptr == 'R')
                    {
                        //force Text Box Reset
                        output += "$FC";
                    }
                    else if (ptr == 'N')
                    {
                        //Line Break
                        output += "$FB";
                    }
                    else if (ptr == ' ')
                    {
                        //space
                        output += "$FA";
                    }
                   
                }

                chars++;
                if (chars == 16)
                {

                    output += "\n" + tab;
                    chars = 0;
                }
                else
                {

                    output += ", ";

                }
                
                counter++;
            }
            output += "$FF";
            return output;
        }

    }
}
