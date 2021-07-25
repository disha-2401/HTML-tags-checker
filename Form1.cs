using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Lab4B
{
    public partial class Form1 : Form
    {
        //Stack that contains all tags from Html file
        readonly Stack HTMLTags = new Stack();

        //List of Tags which will then checked if tags are balanced
        readonly List<string> tags = new List<string>();

        //FileInfo object that contains all info about selected file from OpenFileDialog
        FileInfo fileInfo;

        public Form1()
        {
            InitializeComponent();
            //set Status Label at initialization of form
            FileStatusLabel.Text = "No File Loaded";
        }

        //disable check tags option from Process menu when Form is loaded 
        private void Form1_Load(object sender, EventArgs e)
        {
            //set false since no file is selected yet
            checkTagsToolStripMenuItem.Enabled = false;
        }

        /// <summary>
        /// Method that enables user to select html file and seprates tags from each line from selected file when Load File option from File menu is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //clear list box when new file is loaded
            listBox1.Items.Clear();

            //OpenFileDialog that will filter html documents and restore last opened directory in file explorer
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "HTML files (*.html)|*.html",
                RestoreDirectory = true
            };

            //display OpenFileDialog so that user can select file from explorer
            if (openFileDialog.ShowDialog() == DialogResult.OK) // if user selects "Open" in dialog box
            {
                try
                {
                    //initialize FileInfo object from user selected file
                    fileInfo = new FileInfo(openFileDialog.FileName);

                    //get full path of selected file
                    string fileName = fileInfo.FullName;

                    //check if path is empty and file exist or not
                    if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                    {
                        //check if file is empty
                        if (fileInfo.Length != 0)
                        {
                            // Enabled now so that user can select Check Tags option 
                            checkTagsToolStripMenuItem.Enabled = true;

                            // set label indidcating file status
                            FileStatusLabel.Text = $"Loaded : {openFileDialog.SafeFileName}";

                            // Read every lines from selected file and store in List of strings 
                            List<string> lines = File.ReadAllLines(fileName).ToList<string>();

                            // loop through each lines from list of lines
                            foreach(string line in lines)
                            {
                                // Regex pattern that checks for every substring that start with "<" and ends with ">"
                                string pattern = @"<[^>]+>";

                                // get matches from current line using above Regex pattern
                                MatchCollection matches = Regex.Matches(line, pattern);
                                
                                // execute loop numer of matches found times
                                for (int i = 0; i < matches.Count; i++)
                                {
                                    // add matched tag in tags list
                                    tags.Add(matches[i].ToString());
                                }
                            }
                        }
                        else
                        {
                            // set status label indicating file is empty
                            FileStatusLabel.Text = "Selected File Is Empty";
                        }
                    }
                    else
                    {
                        // set status label indicating no file selected
                        FileStatusLabel.Text = "No File Selected";
                    } 
                }
                catch
                {
                    // show MessageBox when any exception occures during file loading
                    MessageBox.Show("Error In Opening File");
                }
            }
        }

        /// <summary>
        ///  Method that checks for balancing of tags in selected file when Check Tags from Process menu is clicked 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckTagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // List that contains non container tag types
            List<string> nonContainerTags = new List<string>{ "<meta" ,"<link", "<img", "<input", "<br", "<hr", "<!"};
            
            // int indicating no of tab spaces before list item in list box
            int Spaces = -1;
            
            // loop through each string in tags list 
            foreach(string tag in tags)
            { 
                // bool indicating if tag is a non container tag
                bool b = nonContainerTags.Any(x => tag.StartsWith(x));

                // check if tag is non container tag
                if (b)
                {
                    //if first line is non container than print no space at left side
                    if (Spaces < 0)
                    {
                        listBox1.Items.Add($"Found Non Container Tag : {tag}");
                    }
                    // else add into list box with space at left side 
                    else
                    {
                        listBox1.Items.Add($"{String.Concat(Enumerable.Repeat(" ", Spaces * 4))}Found Non Container Tag : {tag}");
                    }
                }

                // check if tag is closing tag
                else if (tag.StartsWith("</"))
                {
                    
                    // add into list box informing that current tag is closing tag and add 4*Spaces white space at left side 
                    listBox1.Items.Add($"{String.Concat(Enumerable.Repeat(" ", Spaces * 4))}Found Closing Tag : {tag}");

                    // since current tag is closing tag decrease spaces value by 1 
                    Spaces--;

                    // Pop a elemnt from stack and assign it to a string variable
                    string tempTag = (string)HTMLTags.Pop();

                    // Regex pattern that will used to remove "<>" and "</>" from string element
                    string pattern = @"(?<=</?)([^ >/]+)";

                    // remove "<>" and "</>" from popped elemnt from stack
                    MatchCollection openingTagTrimmed = Regex.Matches(tempTag, pattern);

                    // remove "</>" from current closing tag 
                    MatchCollection closingTagTrimmed = Regex.Matches(tag.ToLower(), pattern);

                    // check if both the tags are euqal that means tags are balanced 
                    if (openingTagTrimmed[0].Value == closingTagTrimmed[0].Value)
                    {
                        // continue execution to check balancig of next tags
                        continue;
                    }
                    // if tags are not equal it means tags are not balanced
                    else
                    {
                        // call method to set status label indicating that tags are not balanced
                        SetStatus(false);

                        //stop execution of loop when tags aren not found balanced
                        break;
                    }
                }

                // when tag is opening tag 
                else
                {
                    // since current tag is closing tag increase spaces value by 1 
                    Spaces++;

                    // push into Stack 
                    HTMLTags.Push(tag.ToLower());
                    // add into list box informing that current tag is opening tag and add 4*Spaces white space at left side 
                    listBox1.Items.Add($"{String.Concat(Enumerable.Repeat(" ", Spaces * 4))}Found Opening Tag : {tag}");
                }
            }
            // if there element left in stack that means tags are not balanced
            if(HTMLTags.Count > 0)
            {
                // call method to set status label indicating that tags are not balanced
                SetStatus(false);
            }
            // if stack is empty then tags are balanced
            else
            {
                // call method to set status label indicating that tags are balanced
                SetStatus(true);
            }
            //set false since file is processed and user will select next file
            checkTagsToolStripMenuItem.Enabled = false;
        }

        /// <summary>
        /// Method that print tags are balanced or not by given bool value
        /// </summary>
        /// <param name="balanced"></param>
        public void SetStatus(bool balanced)
        {
            // set status label using given bool  
            if (balanced)
            {
                FileStatusLabel.Text = $"{fileInfo.Name} Have Balanced Tags ";
            }
            else
            {
                FileStatusLabel.Text = $"{fileInfo.Name} Does Not Have Balanced Tags ";
            }
        }
        
        /// <summary>
        /// exit applicaion when Exit from File menu is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
