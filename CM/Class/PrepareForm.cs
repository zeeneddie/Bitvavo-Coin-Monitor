﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CM
{
    public class PrepareForm
    {
        #region Properties etc.
        private List<string> CoinName = new();
        public List<string> DgvNames = new();
        public List<string> ChartNames = new();

        private TabControl TabCtrl { get; set; }

        private string DecimalSeperator { get; set; }
        public double WarnPercentage { get; set; }

        private int CbPointX = 3;

        private readonly int CbPointY = 10;
        private int CheckTabCount { get; set; }
        #endregion Properties etc.

        #region constructor
        public PrepareForm()
        {
            GetCoins(); 
            this.DecimalSeperator = CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator;  //move to a static class double code
        }
        #endregion constructor

        private void GetCoins()
        {
            if (StartSession.CheckForInternetConnection())  // First check if there is an active internet connection
            {
                ApplicationDatabase CoinNames = new();
                List<string> AllCoinNames = CoinNames.GetCoinNames();

                if (AllCoinNames != null)  // The first time the application runs and the database is created this is null
                {
                    foreach (string aCoin in AllCoinNames)
                    {
                        CoinName.Add(aCoin);
                    }
                }
            }
        }
        public void CreateTheTabs(TabControl Tctrl)
        {
            TabCtrl = Tctrl;

            CreateTabPages();
            PrepareTabPages();
        }

        #region Create the tab pages
        private void CreateTabPages()
        {
            TabCtrl.TabPages.Clear();  //First remove all tabs

            // Create a Tab for the 24hour difference percentage
            string title = "24 uurs percentage";
            TabPage aTabPageFirst = new(title);
            aTabPageFirst.Name = title;          
            TabCtrl.TabPages.Add(aTabPageFirst);

            //create a tab per coin
            foreach (string value in CoinName)
            {
                title = value;
                TabPage aTabPage = new(title);
                aTabPage.Name = title;              // The name of the tabpage is the coin/market name. "BTC-EUR"
                TabCtrl.TabPages.Add(aTabPage);
            }
        }

        
        #endregion Create the tab pages

        private void PrepareTabPages()
        {
            for (int Tabcount = 0; Tabcount <= TabCtrl.TabCount - 1; Tabcount++)
            {
                if (Tabcount == 0)
                {
                    CreateDgv24hourPercDiffSelectedCoins(Tabcount);
                }
                else
                {
                    CreateSplitContainer(Tabcount);                                                 // Create the splitcontaineers
                    CreateDatagridViewPriceData(Tabcount);                                          // Create DataGrid view
                    CreateChartPanel(Tabcount);                                                     // Create the panel on which the chart will be placed. The control with dockstyle = filled must FIRST be placed on the paren control
                    CreateBottomPanel(Tabcount);                                                    // Create the panel where checkboxes will be placed
                    CreateCheckBox(Tabcount, "CheckBoxShowStartPrice", "Start prijs aan/uit");      // Create a check box

                    CreateCheckBox(Tabcount, "CheckBoxShowStartPrice", "Open prijs aan/uit");       // Create a check box
                    CreateCheckBox(Tabcount, "CheckBoxShowStartPrice", "Sessie hoogste aan/uit");   // Create a check box
                    CreateCheckBox(Tabcount, "CheckBoxShowStartPrice", "Sessie laagste aan/uit");   // Create a check box

                    CreateDatagridViewPriceMonitorPrice(Tabcount);                                  // Create DataGrid view
                    CreateChart(Tabcount);                                                          // Create the charts

                    //add the new components
                    //CreateLabels(Tabcount);  //Perhaps in the future option per tabpage
                    //CreateTextBox(Tabcount);  //Perhaps in the future option per tabpage   
                }
            }
        }

        private void CreateSplitContainer(int Tabcount)
        {
            //Create the first splitcontainer
            SplitContainer spltcontainer = new();
            TabPage tp = TabCtrl.TabPages[Tabcount];

            spltcontainer.Orientation = Orientation.Vertical;
            spltcontainer.SplitterDistance = 60;
            spltcontainer.Dock = DockStyle.Fill;
            spltcontainer.SplitterWidth = 10;
            spltcontainer.BackColor = Color.White;
            spltcontainer.BorderStyle = BorderStyle.FixedSingle;

            tp.Controls.Add(spltcontainer);  //Add the splitcontainer  componentlist of the tabpage

            //Create the second splitcontainer and place in in panel1 of the first splitcontainer.
            SplitContainer spltcontainer1 = new()
            {
                Orientation = Orientation.Horizontal,
                SplitterDistance = 125,
                Dock = DockStyle.Fill,
                SplitterWidth = 10,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            spltcontainer.Panel1.Controls.Add(spltcontainer1);
        }

        private void CreateDatagridViewPriceData(int Tabcount)
        {
            TabPage tp = TabCtrl.TabPages[Tabcount];
            DataGridView dgv = new();
            dgv.Name = "Dgv_1_" + tp.Name;
            dgv.Columns.Add("Onderdeel", "Onderdeel");  //= dgv.Columns[0]
            dgv.Columns.Add("Waarde", "Waarde");        //= dgv.Columns[1]
            dgv.Location = new Point(3, 3);
            dgv.Height = 150;
            dgv.TabIndex = 0;
            dgv.Dock = DockStyle.Fill;

            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToOrderColumns = false;
            dgv.EditMode = DataGridViewEditMode.EditProgrammatically;

            DgvNames.Add(dgv.Name);  //Create a list with newly created datagridview names. 

            dgv.Columns[0].Width = 140;
            dgv.Columns[1].Width = 70;

            //align header(s)
            //dgv.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

            //align cell(s)
            dgv.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            //Add the rows
            dgv.Rows.Add("Start prijs", "0");               //0
            dgv.Rows.Add("", "");                           //1
            dgv.Rows.Add("Huidige prijs", "0");             //2
            dgv.Rows.Add("Verschil [€]", "0");              //3
            dgv.Rows.Add("Verschil [%]", "0");              //4
            dgv.Rows.Add("", "");                           //5
            dgv.Rows.Add(string.Format("Koers bij {0}% winst", WarnPercentage.ToString()), "0");           //6
            dgv.Rows.Add(string.Format("Koers bij {0}% verlies", WarnPercentage.ToString()), "0");         //7
            dgv.Rows.Add("", "");                           //8

            dgv.Rows.Add("Hoogste (sessie)", "0");          //9
            dgv.Rows.Add("Laagste (sessie)", "0");          //10
            dgv.Rows.Add("", "");                           //11

            dgv.Rows.Add("Open (24 uur geleden)", "0");     //12
            dgv.Rows.Add("Hoogste", "0");                   //13
            dgv.Rows.Add("Laagste", "0");                   //14
            dgv.Rows.Add("Volume", "0");                    //15
            dgv.Rows.Add("Volume quote", "0");              //16
            dgv.Rows.Add("Bieden", "0");                    //17
            dgv.Rows.Add("Vraag", "0");                     //18
            dgv.Rows.Add("Bied grootte", "0");              //19
            dgv.Rows.Add("Vraag grootte", "0");             //20

            dgv.Rows[1].Height = 7;
            dgv.Rows[5].Height = 7;
            dgv.Rows[8].Height = 7;
            dgv.Rows[11].Height = 7;

            dgv.Rows[1].Cells[0].Style.BackColor = Color.LightGray;
            dgv.Rows[1].Cells[1].Style.BackColor = Color.LightGray;

            dgv.Rows[5].Cells[0].Style.BackColor = Color.LightGray;
            dgv.Rows[5].Cells[1].Style.BackColor = Color.LightGray;

            dgv.Rows[8].Cells[0].Style.BackColor = Color.LightGray;
            dgv.Rows[8].Cells[1].Style.BackColor = Color.LightGray;

            dgv.Rows[11].Cells[0].Style.BackColor = Color.LightGray;
            dgv.Rows[11].Cells[1].Style.BackColor = Color.LightGray;

            dgv.RowHeadersVisible = false;

            foreach (Control c in tp.Controls)
            {
                if (c.GetType() == typeof(SplitContainer))
                {
                    SplitContainer splt1 = (SplitContainer)c;
                    foreach (Control c1 in splt1.Panel1.Controls)
                    {
                        if (c1.GetType() == typeof(SplitContainer))
                        {
                            SplitContainer splt2 = (SplitContainer)c1;
                            splt2.Panel1.Controls.Add(dgv);
                        }
                    }
                }
            }
        }
        private void CreateDatagridViewPriceMonitorPrice(int Tabcount)
        {
            DataGridView dgv = new();
            TabPage tp = TabCtrl.TabPages[Tabcount];
            dgv.Name = "Dgv_2_" + tp.Name;
            dgv.Columns.Add("Prijs", "Prijs");
            dgv.Columns.Add("Empty_1", "    ");
            dgv.Columns.Add("Datum_tijd", "Datum - tijd");

            dgv.Columns[0].ToolTipText = "Huidige prijs.";
            dgv.Columns[1].ToolTipText = "Trend ten opzichte van de vorige prijs.";

            dgv.Columns[0].Width = 60;
            dgv.Columns[1].Width = 20;
            dgv.Columns[2].Width = 120;

            dgv.Location = new Point(3, 3);
            dgv.Height = 150;
            dgv.TabIndex = 0;
            dgv.Dock = DockStyle.Fill;

            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToOrderColumns = false;
            dgv.EditMode = DataGridViewEditMode.EditProgrammatically;
            dgv.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.RowHeadersVisible = false;

            DgvNames.Add(dgv.Name);  //Create a list with newly created datagridview names. 

           //add the datagrid view to the splitcontainer (left panel);

            foreach (Control c in tp.Controls)
            {
                if (c.GetType() == typeof(SplitContainer))
                {
                    SplitContainer splt1 = (SplitContainer)c;
                    foreach (Control c1 in splt1.Panel1.Controls)
                    {
                        if (c1.GetType() == typeof(SplitContainer))
                        {
                            SplitContainer splt2 = (SplitContainer)c1;
                            splt2.Panel2.Controls.Add(dgv);
                        }
                    }
                }
            }
        }
        private void CreateDgv24hourPercDiffSelectedCoins(int Tabcount)
        {
            DataGridView dgv = new();
            TabPage tp = TabCtrl.TabPages[Tabcount];
            dgv.Name = "Dgv_DifPerc24hour";
            dgv.Columns.Add("Coin", "Coin");
            dgv.Columns.Add("Percentage", "Percentage");

            dgv.Columns[0].Width = 70;
            dgv.Columns[1].Width = 80;            

            dgv.Location = new Point(3, 3);
            dgv.Height = tp.Height;
            dgv.Width = 193;
            dgv.TabIndex = 0;

            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToOrderColumns = true;
            dgv.EditMode = DataGridViewEditMode.EditProgrammatically;
            dgv.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.RowHeadersVisible = false;

            dgv.Anchor = (AnchorStyles.Left |  AnchorStyles.Top | AnchorStyles.Bottom);

            DgvNames.Add(dgv.Name);  //Create a list with newly created datagridview names. 

            tp.Controls.Add(dgv);
        }
        



        private void CreateChart(int Tabcount)
        {
            //source: https://timbar.blogspot.com/2012/04/creating-chart-programmatically-in-c.html
            //source: https://hirenkhirsaria.blogspot.com/2012/06/dynamically-creating-piebar-chart-in-c.html

            TabPage tp = TabCtrl.TabPages[Tabcount];
            string CoinName = tp.Name;

            // Chart does not exists in .net 5 !?!? used a nuget download
            System.Windows.Forms.DataVisualization.Charting.Chart aChart = new();
            aChart.Size = new Size(100, 100);
            aChart.Name = "Chart_" + tp.Name;
            aChart.Titles.Add(CoinName);
            aChart.Location = new Point(3, 3);
            aChart.Cursor = Cursors.Cross;


            System.Windows.Forms.DataVisualization.Charting.ChartArea ChrtArea = new("ChartArea_"+ CoinName);
            ChrtArea.Area3DStyle.Enable3D = false;
            aChart.ChartAreas.Add(ChrtArea);
            ChrtArea.AxisX.Title = "Tijd";
            ChrtArea.AxisY.Title = "Koers";

            //TODO create a function for this
            //The current coin price
            System.Windows.Forms.DataVisualization.Charting.Series series = new("Series_" + CoinName)
            {
                Name = "Series_" + CoinName,
                ChartArea = "ChartArea_" + CoinName
            };
            aChart.Series.Add(CoinName);            
            aChart.Series[CoinName].Enabled = true;

            aChart.Series[CoinName].MarkerColor = Color.Red;
            aChart.Series[CoinName].MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            aChart.Series[CoinName].MarkerSize = 5;

            //Start_Prijs
            System.Windows.Forms.DataVisualization.Charting.Series SeriesStartPrice = new("Start_Prijs_" + CoinName);
            SeriesStartPrice.Name = "Start_Prijs_" + CoinName;
            SeriesStartPrice.LegendText = "Start_Prijs";
            aChart.Series.Add(SeriesStartPrice);
            aChart.Series["Start_Prijs_" + CoinName].Enabled = true;

            //Open_Prijs
            System.Windows.Forms.DataVisualization.Charting.Series SeriesOpenPrice = new("Open_Prijs_" + CoinName);
            SeriesOpenPrice.Name = "Open_Prijs_" + CoinName;
            SeriesOpenPrice.LegendText = "Open_Prijs";
            aChart.Series.Add(SeriesOpenPrice);
            aChart.Series["Open_Prijs_" + CoinName].Enabled = true;

            //Sessie_Hoogste_Prijs
            System.Windows.Forms.DataVisualization.Charting.Series SeriesSessionHighestPrice = new("Sessie_Hoogste_Prijs_" + CoinName);
            SeriesSessionHighestPrice.Name = "Sessie_Hoogste_Prijs_" + CoinName;
            SeriesSessionHighestPrice.LegendText = "Sessie_Hoogste";
            aChart.Series.Add(SeriesSessionHighestPrice);
            aChart.Series["Sessie_Hoogste_Prijs_" + CoinName].Enabled = true;

            //Sessie_Laagste_Prijs
            System.Windows.Forms.DataVisualization.Charting.Series SeriesSessionLowestPrice = new("Sessie_Laagste_Prijs_" + CoinName);
            SeriesSessionLowestPrice.Name = "Sessie_Laagste_Prijs_" + CoinName;
            SeriesSessionLowestPrice.LegendText = "Sessie_Laagste";
            aChart.Series.Add(SeriesSessionLowestPrice);
            aChart.Series["Sessie_Laagste_Prijs_" + CoinName].Enabled = true;

            //create the legend           
            aChart.Legends.Add(new System.Windows.Forms.DataVisualization.Charting.Legend("Price"));
            aChart.Legends[0].TableStyle = System.Windows.Forms.DataVisualization.Charting.LegendTableStyle.Auto;
            aChart.Legends[0].Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            aChart.Legends[0].Alignment = System.Drawing.StringAlignment.Center;
            aChart.Series[0].Legend = "Price";




            aChart.ChartAreas[0].AxisX.IsStartedFromZero = false;
            aChart.ChartAreas[0].AxisY.IsStartedFromZero = false;         

            aChart.Visible = true;
            aChart.Invalidate();

            ChartNames.Add(aChart.Name);

            foreach (Control c in tp.Controls)
            {
                if (c.GetType() == typeof(SplitContainer))
                {
                    SplitContainer splt1 = (SplitContainer)c;

                    foreach (Control c1 in splt1.Panel2.Controls)
                    {
                        if (c1.GetType() == typeof(Panel))
                        {
                            if (c1.Name == "ChartPanel")
                            {
                                Panel aPanel = (Panel)c1;

                                aPanel.Controls.Add(aChart);

                                aChart.Dock = DockStyle.Fill;
                            } 
                        }
                    }
                    //splt1.Panel2.Controls.Add(aChart);
                }
            }
        }
        

        /*
        private void CreateLabels(int Tabcount)
        {
            Label lb = new();

            lb.Name = "LabelWarnPrec";
            lb.AutoSize = true;
            lb.Text = "Waarschuwen bij een stijging/daling van :";
            lb.Location = new Point(3, 6);

            PlaceControleOnSpltcontainerTwo(lb, Tabcount);           
        }
        */

        #region Textbox warning precentage

        /*private void CreateTextBox(int Tabcount)
        {
            TextBox tb = new();
            tb.Name = "TextBoxWarnPrec";
            tb.Text = "1";
            tb.TextAlign = HorizontalAlignment.Right;
            tb.Location = new Point(235,3);
            tb.Size = new Size(40,23);

            tb.KeyPress += new KeyPressEventHandler(tb_KeyPress);

            PlaceControleOnSpltcontainerTwo(tb, Tabcount);
        }

        private void tb_KeyPress(object sender, KeyPressEventArgs e)
        {
            KeyPresstextBox(sender, e);
        }

        private void KeyPresstextBox(object sender, KeyPressEventArgs e)
        {
            char seperator = this.DecimalSeperator[0];

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                    (e.KeyChar != seperator))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == seperator) && ((sender as TextBox).Text.IndexOf(seperator) > -1))
            {
                e.Handled = true;
            }
        }*/

        #endregion Textbox warning precentage
        /*
        private void PlaceControleOnSpltcontainerTwo(Control NewCntrl, int Tabcount)
        {
            TabPage tp = TabCtrl.TabPages[Tabcount];

            foreach (Control c in tp.Controls)
            {
                if (c.GetType() == typeof(SplitContainer))
                {
                    SplitContainer splt1 = (SplitContainer)c;
                    foreach (Control c1 in splt1.Panel1.Controls)
                    {
                        if (c1.GetType() == typeof(SplitContainer))
                        {
                            SplitContainer splt2 = (SplitContainer)c1;
                            splt2.Panel1.Controls.Add(NewCntrl);
                        }
                    }   
                }
            }
        }*/

        private void PlaceControleOnSpltcontainerOne(Control NewCntrl, int Tabcount)
        {
            TabPage tp = TabCtrl.TabPages[Tabcount];
            foreach (Control c in tp.Controls)
            {
                if (c.GetType() == typeof(SplitContainer))
                {
                    SplitContainer splt2 = (SplitContainer)c;
                    splt2.Panel2.Controls.Add(NewCntrl);
                }
            }
        }
        private void PlaceControlOnSpltcontainerOnePanel(Control NewCntrl, int Tabcount)
        {
            TabPage tp = TabCtrl.TabPages[Tabcount];
            foreach (Control c in tp.Controls)
            {
                if (c.GetType() == typeof(SplitContainer))
                {
                    SplitContainer splt1 = (SplitContainer)c;
                    foreach (Control c1 in splt1.Panel2.Controls)
                    {
                        if (c1.GetType() == typeof(Panel))
                        {
                            Panel aPanel = (Panel)c1;
                            if (aPanel.Name == "BottomPanel")
                            {
                                aPanel.Controls.Add(NewCntrl);
                            }
                        }
                    }
                }
            }
        }

        private void CreateBottomPanel(int Tabcount)
        {
            Panel BottomPanel = new();
            BottomPanel.Name = "BottomPanel";
            BottomPanel.Text = string.Empty;
            BottomPanel.Location = new Point(3, 50);
            BottomPanel.Size = new Size(50, 32);
            BottomPanel.BackColor = Color.LightSteelBlue;
            BottomPanel.Dock = DockStyle.Bottom;

            PlaceControleOnSpltcontainerOne(BottomPanel, Tabcount);
        }
        private void CreateChartPanel(int Tabcount)

        {
            Panel ChartPanel = new();
            ChartPanel.Name = "ChartPanel";
            ChartPanel.Text = string.Empty;
            ChartPanel.Location = new Point(3, 50);
            ChartPanel.Size = new Size(50, 32);
            ChartPanel.BackColor = Color.LightGray;
            ChartPanel.Dock = DockStyle.Fill;

            PlaceControleOnSpltcontainerOne(ChartPanel, Tabcount);
        }

        private void CreateCheckBox(int Tabcount, string Name, string Text)
        {
            if (Tabcount != CheckTabCount)  // Next tab page, reset Y
            {
                CheckTabCount = Tabcount;
                CbPointX = 3;
            }

            CheckBox Cb = new();
            Cb.Name = Name;
            Cb.Text = Text;
            Cb.Checked = true;
            Cb.AutoSize = true;
            if (CbPointX == 0)
            {
                Cb.Location = new Point(CbPointX, CbPointY);
            }
            else
            {
                Cb.Location = new Point(CbPointX, CbPointY);
            }
            CbPointX += 150;

            Cb.Size = new Size(82, 19);
            Cb.Anchor = (AnchorStyles.Left);

            PlaceControlOnSpltcontainerOnePanel(Cb, Tabcount);
        }
    }
}

