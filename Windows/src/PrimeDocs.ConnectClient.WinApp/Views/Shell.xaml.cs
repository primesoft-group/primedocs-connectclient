﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Schema;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using PrimeDocs.ConnectClient.WinApp.XHelper;
using PrimeDocs.ConnectClient.WinApp.XmlCompletion;
using MahApps.Metro.Controls.Dialogs;

namespace PrimeDocs.ConnectClient.WinApp.Views
{
    public partial class Shell : UserControl
    {
        private CompletionWindow currentlyActiveCompletionWindow;

        public Shell()
        {
            InitializeComponent();
            this.DataContext = new ViewModel.ShellViewModel(DialogCoordinator.Instance);

            XmlSchemaSet schemas = new XmlSchemaSet();
            var asm = Assembly.GetExecutingAssembly();
            using (var stream = asm.GetManifestResourceStream("PrimeDocs.ConnectClient.WinApp.XSD.PrimeDocsValidation.xsd"))
            {
                if (stream != null)
                {
                    var reader = new StreamReader(stream);
                    var xmlString = reader.ReadToEnd();
                    schemas.Add("", XmlReader.Create(new StringReader(xmlString)));
                }
            }
            schemas.Compile();
            XsdInformation = XHelper.XsdParser.AnalyseSchema(schemas);

            textEditor.TextArea.Name = "TextArea";
            textEditor.TextArea.TextEntered += TextArea_TextEntered;

            textEditorClient.TextArea.Name = "TextAreaClient";
            textEditorClient.TextArea.TextEntered += TextArea_TextEntered;
        }

        private void TextBox_PreviewDrop(object sender, DragEventArgs e)
        {
            ((ViewModel.ShellViewModel)this.DataContext).PreviewDrop(e);
        }

        private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = ((ViewModel.ShellViewModel)this.DataContext).PreviewDragOver(e);
        }
        public List<XsdElementInformation> XsdInformation { get; set; }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            TextEditor _editor = new TextEditor();
            switch (((TextArea)sender).Name)
            {
                case "TextArea":
                    _editor = textEditor;
                    break;
                case "TextAreaClient":
                    _editor = textEditorClient;
                    break;
            }

            try
            {

                switch (e.Text)
                {
                    case ">":
                        {
                            //auto-insert closing element
                            int offset = _editor.CaretOffset;
                            string s = XHelper.XmlParser.GetElementAtCursor(_editor.Text, offset - 1);
                            if (!string.IsNullOrWhiteSpace(s) && !s.StartsWith("!--"))
                            {
                                if (!XHelper.XmlParser.IsClosingElement(_editor.Text, offset - 1, s))
                                {
                                    string endElement = "</" + s + ">";
                                    var rightOfCursor = _editor.Text.Substring(offset, Math.Max(0, Math.Min(endElement.Length + 50, _editor.Text.Length) - offset - 1)).TrimStart();
                                    if (!rightOfCursor.StartsWith(endElement))
                                    {
                                        _editor.TextArea.Document.Insert(offset, endElement);
                                        _editor.CaretOffset = offset;
                                    }
                                }
                            }
                            break;
                        }
                    case "/":
                        {
                            int offset = _editor.CaretOffset;
                            if (_editor.Text.Length > offset + 2 && _editor.Text[offset] == '>')
                            {
                                //remove closing tag if exist
                                string s = XHelper.XmlParser.GetElementAtCursor(_editor.Text, offset - 1);
                                if (!string.IsNullOrWhiteSpace(s))
                                {
                                    //search closing end tag. Element must be empty (whitespace allowed)  
                                    //"<hallo>  </hallo>" --> enter '/' --> "<hallo/>  "
                                    string expectedEndTag = "</" + s + ">";
                                    for (int i = offset + 1; i < _editor.Text.Length - expectedEndTag.Length + 1; i++)
                                    {
                                        if (!char.IsWhiteSpace(_editor.Text[i]))
                                        {
                                            if (_editor.Text.Substring(i, expectedEndTag.Length) == expectedEndTag)
                                            {
                                                //remove already existing endTag
                                                _editor.Document.Remove(i, expectedEndTag.Length);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case "<":
                        var parentElement = XHelper.XmlParser.GetParentElementPath(_editor.Text);
                        var elementAutocompleteList = ProvidePossibleElementsAutocomplete(parentElement);

                        InvokeCompletionWindow(elementAutocompleteList, false, _editor);

                        break;
                    case " ":
                        {
                            var currentElement = XHelper.XmlParser.GetActiveElementStartPath(_editor.Text, _editor.CaretOffset);
                            var attributeautocompletelist = ProvidePossibleAttributesAutocomplete(currentElement);
                            InvokeCompletionWindow(attributeautocompletelist, true, _editor);
                            break;
                        }

                }
            }
            catch (Exception exc)
            {

            }

            if (e.Text.Length > 0)
            {
                char c = e.Text[0];
                if (!(char.IsLetterOrDigit(c) || char.IsPunctuation(c)))
                {
                    e.Handled = true;
                }
            }

            // close current CompletionWindow if there is no entry (only on xml completion data)
            if (currentlyActiveCompletionWindow != null)
            {
                if (currentlyActiveCompletionWindow != null && !currentlyActiveCompletionWindow.CompletionList.ListBox.HasItems
                    && currentlyActiveCompletionWindow.CompletionList.CompletionData != null && currentlyActiveCompletionWindow.CompletionList.CompletionData.FirstOrDefault() is XmlCompletionData)
                {
                    currentlyActiveCompletionWindow.Close();
                }
            }
        }

        private void InvokeCompletionWindow(List<Tuple<string, string>> elementAutocompleteList, bool isAttribute, TextEditor editor)
        {
            if (elementAutocompleteList.Count > 0)
            {
                var completionWindow = new CompletionWindow(editor.TextArea);
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                if (elementAutocompleteList.Any())
                {
                    foreach (var autocompleteelement in elementAutocompleteList)
                    {
                        data.Add(new XmlCompletionData(autocompleteelement.Item1, autocompleteelement.Item2,
                            isAttribute));
                    }

                    completionWindow.Width = 280;
                    ShowComplitionWindow(completionWindow);
                }
            }
        }

        private void ShowComplitionWindow(CompletionWindow paramCompletionWindow)
        {
            currentlyActiveCompletionWindow = paramCompletionWindow;

            paramCompletionWindow.Show();

            paramCompletionWindow.Closed += delegate { paramCompletionWindow = null; currentlyActiveCompletionWindow = null; };
        }

        public List<Tuple<string, string>> ProvidePossibleElementsAutocomplete(XmlElementInformation path)
        {
            List<Tuple<string, string>> result = new List<Tuple<string, string>>();

            if (path.IsEmpty)
            {
                var xsdResultForGivenElementPath = XsdInformation.FirstOrDefault(x => x.IsRoot);

                if (xsdResultForGivenElementPath != null)
                {
                    result.Add(new Tuple<string, string>(xsdResultForGivenElementPath.Name, xsdResultForGivenElementPath.DataType));
                }
            }
            else
            {
                StringBuilder xpath = new StringBuilder();
                xpath.Append("/");
                foreach (var element in path.Elements)
                {
                    xpath.Append("/" + element.Name);
                }

                var xsdResultForGivenElementPath = XsdInformation.FirstOrDefault(x => x.XPathLikeKey.ToLowerInvariant() == xpath.ToString().ToLowerInvariant());

                if (xsdResultForGivenElementPath != null)
                {
                    foreach (var xsdInformationElement in xsdResultForGivenElementPath.Elements)
                    {
                        result.Add(new Tuple<string, string>(xsdInformationElement.Name, xsdInformationElement.DataType));
                    }
                }
            }


            return result;
        }

        public List<Tuple<string, string>> ProvidePossibleAttributesAutocomplete(XmlElementInformation path)
        {
            List<Tuple<string, string>> result = new List<Tuple<string, string>>();

            if (path.IsEmpty)
            {
                var xsdResultForGivenElementPath = XsdInformation.FirstOrDefault(x => x.IsRoot);

                if (xsdResultForGivenElementPath != null)
                {
                    foreach (var xsdInformationAttribute in xsdResultForGivenElementPath.Attributes)
                    {
                        result.Add(new Tuple<string, string>(xsdInformationAttribute.Name, xsdInformationAttribute.DataType));
                    }
                }
            }
            else
            {
                StringBuilder xpath = new StringBuilder();
                xpath.Append("/");
                foreach (var element in path.Elements)
                {
                    xpath.Append("/" + element.Name);
                }

                var xsdResultForGivenElementPath = XsdInformation.FirstOrDefault(x => x.XPathLikeKey.ToLowerInvariant() == xpath.ToString().ToLowerInvariant());

                if (xsdResultForGivenElementPath != null)
                {
                    foreach (var xsdInformationAttribute in xsdResultForGivenElementPath.Attributes)
                    {
                        result.Add(new Tuple<string, string>(xsdInformationAttribute.Name, xsdInformationAttribute.DataType));
                    }
                }
            }


            return result;
        }

        private void TextBox_VisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Keyboard.Focus(textBox);
        }
    }
}
