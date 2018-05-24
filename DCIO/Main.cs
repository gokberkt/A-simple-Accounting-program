using MetroFramework;
using MetroFramework.Forms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace DCIO
{
    public partial class Main : MetroForm
    {
        dcDBEntities db = new dcDBEntities();
        BackgroundWorker bgw = new BackgroundWorker();


        public Main()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

        }

        private void Main_Load(object sender, EventArgs e)
        {
            metroTabControl1.SelectedTab = metroTabPage1;
            metroTabControl2.SelectedTab = metroTabPage5;
            metroTabControl3.SelectedTab = metroTabPage12;
            metroTabControl4.SelectedTab = metroTabPage15;

            lvIncomes.Columns.Add("Tarih", 120);
            lvIncomes.Columns.Add("Açıklama", 332);
            lvIncomes.Columns.Add("Tutar", 110);
            lvIncomes.View = View.Details;
            lvIncomes.AllowColumnReorder = true;
            FillListView();
            UpdateTotalIncome();


            lvOutcome.Columns.Add("Tarih", 120);
            lvOutcome.Columns.Add("Açıklama", 332);
            lvOutcome.Columns.Add("Tutar", 110);
            lvOutcome.View = View.Details;
            lvOutcome.AllowColumnReorder = true;
            FillListViewOutCome();
            UpdateTotalOutcome();

            lvEmpList.Columns.Add("Ad Soyad", 250);
            lvEmpList.Columns.Add("Branş", 160);
            lvEmpList.Columns.Add("Maaş", 110);
            lvEmpList.View = View.Details;
            lvEmpList.AllowColumnReorder = true;
            FillListViewEmployees();
            UpdateTotalEmployees();

            lwPayments.Columns.Add("Ad Soyad", 250);
            lwPayments.Columns.Add("Ödeme Türü", 120);
            lwPayments.Columns.Add("Miktar", 100);
            lwPayments.Columns.Add("Tarih", 100);
            lwPayments.View = View.Details;
            lwPayments.AllowColumnReorder = true;
            FillListViewPayments();

            lvNewPhotos.Columns.Add("Tarih", 190);
            lvNewPhotos.Columns.Add("Saat", 150);
            lvNewPhotos.View = View.Details;
            lvNewPhotos.AllowColumnReorder = true;
            FillListViewNewPhotos();
            int notCount = NotificationCount();
            if (notCount > 0)
            {
                notTimer.Interval = 350;
                notTimer.Tick += notTimer_Tick;
                notTimer.Start();
            }

            lvOldPhotos.Columns.Add("Tarih", 190);
            lvOldPhotos.Columns.Add("Saat", 150);
            lvOldPhotos.View = View.Details;
            lvOldPhotos.AllowColumnReorder = true;
            FillListViewOldPhotos();

            FillChart();
            FillYearlyOutcome();
            FillYearlyRevenue();

            System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
            ToolTip1.SetToolTip(this.btnRefresh, "Yenile");
            ToolTip1.SetToolTip(this.btnRefreshOutcome, "Yenile");
            ToolTip1.SetToolTip(this.btnRefreshRevenue, "Yenile");

            FillComboBoxes();
            FillSettinsTextBoxes();

            bgw.DoWork += Bgw_DoWork;

            
        }


        Timer notTimer = new Timer();
        private void notTimer_Tick(object sender, EventArgs e)
        {
            lblNoficationCounter.Visible = !lblNoficationCounter.Visible;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }


        //Gelir Sekmesi Başlangıç//

        private void btnAddIncome_Click(object sender, EventArgs e)
        {
            Incomes inc = new Incomes();
            if (dtpAddInc.Value > DateTime.Now)
            {
                MetroMessageBox.Show(this, "İleri tarihli kayıt giremezsiniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtAddDescp.Text) || string.IsNullOrWhiteSpace(txtAddAmount.Text))
            {
                MetroMessageBox.Show(this, "Açıklama ve ya Miktar boş geçilemez !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            try
            {
                inc.Date = dtpAddInc.Value;
                inc.Amount = Convert.ToDecimal(txtAddAmount.Text);
                inc.Description = txtAddDescp.Text;
                inc.CreatedDate = DateTime.Now;
                inc.Status = true;
                db.Incomes.Add(inc);
                db.SaveChanges();
                Clear();
                FillListView();
                UpdateTotalIncome();
                MetroMessageBox.Show(this, "Kayıt başarıyla eklendi !", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void UpdateTotalIncome()
        {
            string inc = db.Incomes.Where(x => x.Status == true).Sum(x => x.Amount).ToString();

            lblTotalIncome.Text = string.Format("{0:C}", Convert.ToDecimal(inc));
        }


        private void btnChangeInc_Click(object sender, EventArgs e)
        {

            try
            {
                if (lvIncomes.SelectedItems.Count == 0)
                {
                    MetroMessageBox.Show(this, "Kayıt Seçmediniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
                int id = Convert.ToInt32(lblIncID.Text);
                Incomes inc = db.Incomes.FirstOrDefault(x => x.ID == id);
                if (inc != null)
                {
                    DialogResult dr = MetroMessageBox.Show(this, "Seçili kaydı güncellemek istediğinizden emin misiniz ?", "Uyarı", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                    if (dr == DialogResult.Yes)
                    {
                        inc.Date = dtpChangeInc.Value;
                        inc.Description = txtChangeDesc.Text;
                        inc.Amount = Convert.ToDecimal(txtChangeAmount.Text);
                        db.SaveChanges();
                        FillListView();
                        Clear();
                        UpdateTotalIncome();
                        MetroMessageBox.Show(this, "Güncelleme İşlemi Başarılı !", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else if (dr == DialogResult.No)
                    {
                        MetroMessageBox.Show(this, "Güncelleme işlemi iptal edildi !", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        Clear();
                        FillListView();
                    }
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void btnDeleteInc_Click(object sender, EventArgs e)
        {
            try
            {
                if (lvIncomes.SelectedItems.Count == 0)
                {
                    MetroMessageBox.Show(this, "Kayıt Seçmediniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
                int id = Convert.ToInt32(lblIncID.Text);
                Incomes inc = db.Incomes.FirstOrDefault(x => x.ID == id);
                if (inc != null)
                {
                    DialogResult dr = MetroMessageBox.Show(this, "Seçili kaydı silmek istediğinizden emin misiniz ?", "Uyarı", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                    if (dr == DialogResult.Yes)
                    {
                        inc.Status = false;
                        db.SaveChanges();
                        FillListView();
                        Clear();
                        UpdateTotalIncome();
                        MetroMessageBox.Show(this, "Silme İşlemi Başarılı !", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else if (dr == DialogResult.No)
                    {
                        MetroMessageBox.Show(this, "Silme işlemi iptal edildi !", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        Clear();
                        FillListView();
                    }
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void lvIncomes_Click(object sender, EventArgs e)
        {
            lblIncID.Text = lvIncomes.SelectedItems[0].Tag.ToString();
            int id = Convert.ToInt32(lblIncID.Text);
            Incomes inc = db.Incomes.FirstOrDefault(x => x.ID == id);

            dtpChangeInc.Value = (DateTime)inc.Date;
            txtChangeAmount.Text = string.Format("{0:f2}", inc.Amount);
            txtChangeDesc.Text = inc.Description;
        }

        //Gelir Sekmesi Bitiş//

        //Gider Sekmesi Başlangıç//

        private void btnAddOutcome_Click(object sender, EventArgs e)
        {
            Outcomes outcome = new Outcomes();
            if (dtpAddOutcome.Value > DateTime.Now)
            {
                MetroMessageBox.Show(this, "İleri tarihli kayıt giremezsiniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtAddOutcomeDesc.Text) || string.IsNullOrWhiteSpace(txtAddOutcomeAmount.Text))
            {
                MetroMessageBox.Show(this, "Açıklama ve ya Miktar boş geçilemez !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            try
            {
                outcome.Date = dtpAddOutcome.Value;
                outcome.Amount = Convert.ToDecimal(txtAddOutcomeAmount.Text);
                outcome.Description = txtAddOutcomeDesc.Text;
                outcome.CreatedDate = DateTime.Now;
                outcome.Status = true;
                db.Outcomes.Add(outcome);
                db.SaveChanges();
                Clear();
                FillListViewOutCome();
                UpdateTotalOutcome();
                MetroMessageBox.Show(this, "Kayıt başarıyla eklendi !", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateTotalOutcome()
        {
            string outcome = db.Outcomes.Where(x => x.Status == true).Sum(x => x.Amount).ToString();

            lblTotalOutcome.Text = string.Format("{0:C}", Convert.ToDecimal(outcome));
        }

        private void lvOutcome_Click(object sender, EventArgs e)
        {
            lblOutID.Text = lvOutcome.SelectedItems[0].Tag.ToString();
            int id = Convert.ToInt32(lblOutID.Text);
            Outcomes outcome = db.Outcomes.FirstOrDefault(x => x.ID == id);

            dtpChangeOutcome.Value = (DateTime)outcome.Date;
            txtChangeOutcomeAmount.Text = string.Format("{0:f2}", outcome.Amount);
            txtChangeOutcomeDesc.Text = outcome.Description;
        }

        private void btnChangeOutcome_Click(object sender, EventArgs e)
        {
            try
            {
                if (lvOutcome.SelectedItems.Count == 0)
                {
                    MetroMessageBox.Show(this, "Kayıt Seçmediniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
                int id = Convert.ToInt32(lblOutID.Text);
                Outcomes outcomes = db.Outcomes.FirstOrDefault(x => x.ID == id);
                if (outcomes != null)
                {
                    DialogResult dr = MetroMessageBox.Show(this, "Seçili kaydı güncellemek istediğinizden emin misiniz ?", "Uyarı", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                    if (dr == DialogResult.Yes)
                    {
                        outcomes.Date = dtpChangeOutcome.Value;
                        outcomes.Description = txtChangeOutcomeDesc.Text;
                        outcomes.Amount = Convert.ToDecimal(txtChangeOutcomeAmount.Text);
                        db.SaveChanges();
                        FillListViewOutCome();
                        Clear();
                        UpdateTotalOutcome();
                        MetroMessageBox.Show(this, "Güncelleme İşlemi Başarılı !", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else if (dr == DialogResult.No)
                    {
                        MetroMessageBox.Show(this, "Güncelleme işlemi iptal edildi !", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        Clear();
                        FillListViewOutCome();
                    }
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void btnDeleteOutcome_Click(object sender, EventArgs e)
        {
            try
            {
                if (lvOutcome.SelectedItems.Count == 0)
                {
                    MetroMessageBox.Show(this, "Kayıt Seçmediniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
                int id = Convert.ToInt32(lblOutID.Text);
                Outcomes outcome = db.Outcomes.FirstOrDefault(x => x.ID == id);
                if (outcome != null)
                {
                    DialogResult dr = MetroMessageBox.Show(this, "Seçili kaydı silmek istediğinizden emin misiniz ?", "Uyarı", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                    if (dr == DialogResult.Yes)
                    {
                        outcome.Status = false;
                        db.SaveChanges();
                        FillListViewOutCome();
                        Clear();
                        UpdateTotalOutcome();
                        MetroMessageBox.Show(this, "Silme İşlemi Başarılı !", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else if (dr == DialogResult.No)
                    {
                        MetroMessageBox.Show(this, "Silme işlemi iptal edildi !", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        Clear();
                        FillListViewOutCome();
                    }
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        //Gider Sekmesi Bitiş//



        ////////////////// Genel Metotlar Başlangıç////////////////////
        private void FillListView()
        {
            try
            {
                lvIncomes.Items.Clear();
                foreach (Incomes item in db.Incomes.Where(x => x.Status == true).OrderBy(x => x.Date).ToList())
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Tag = item.ID;
                    lvi.Text = string.Format("{0:dd/MM/yyyy}", item.Date);
                    lvi.SubItems.Add(item.Description);
                    lvi.SubItems.Add(string.Format("{0:C}", item.Amount));
                    lvIncomes.Items.Add(lvi);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FillListViewOutCome()
        {
            try
            {
                lvOutcome.Items.Clear();
                foreach (Outcomes item in db.Outcomes.Where(x => x.Status == true).OrderBy(x => x.Date).ToList())
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Tag = item.ID;
                    lvi.Text = string.Format("{0:dd/MM/yyyy}", item.Date);
                    lvi.SubItems.Add(item.Description);
                    lvi.SubItems.Add(string.Format("{0:C}", item.Amount));
                    lvOutcome.Items.Add(lvi);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FillListViewEmployees()
        {
            try
            {
                lvEmpList.Items.Clear();
                foreach (Employees item in db.Employees.Where(x => x.Status == true).OrderBy(x => x.Name).ToList())
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Tag = item.ID;
                    lvi.Text = item.Name;
                    lvi.SubItems.Add(item.Branch);
                    lvi.SubItems.Add(string.Format("{0:C}", item.Salary));
                    lvEmpList.Items.Add(lvi);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FillListViewPayments()
        {
            try
            {
                lwPayments.Items.Clear();
                foreach (Payments item in db.Payments.Where(x => x.Status == true).OrderBy(x => x.Date).ToList())
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Tag = item.ID;
                    lvi.Text = item.Employees.Name;
                    lvi.SubItems.Add(item.PaymentTypes.Type);
                    lvi.SubItems.Add(string.Format("{0:C}", item.Amount));
                    lvi.SubItems.Add(string.Format("{0:dd/MM/yyyy}", item.Date));
                    lwPayments.Items.Add(lvi);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FillListViewNewPhotos()
        {
            try
            {
                lvNewPhotos.Items.Clear();
                foreach (CamLogs item in db.CamLogs.Where(x => x.Status == true && x.isSeen == false).OrderBy(x => x.CreatedDate).ToList())
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Tag = item.Name;
                    lvi.Text = string.Format("{0:dd/MM/yyyy}", item.CreatedDate);
                    lvi.SubItems.Add(string.Format("{0:HH/mm/ss}", item.CreatedDate));
                    lvNewPhotos.Items.Add(lvi);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FillListViewOldPhotos()
        {
            try
            {
                lvOldPhotos.Items.Clear();
                foreach (CamLogs item in db.CamLogs.Where(x => x.Status == true && x.isSeen == true).OrderBy(x => x.CreatedDate).ToList())
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Tag = item.Name;
                    lvi.Text = string.Format("{0:dd/MM/yyyy}", item.CreatedDate);
                    lvi.SubItems.Add(string.Format("{0:HH/mm/ss}", item.CreatedDate));
                    lvOldPhotos.Items.Add(lvi);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Clear()
        {
            foreach (Control item in groupBox1.Controls)
            {
                if (item is TextBox)
                {
                    TextBox text = (TextBox)item;
                    text.Clear();
                }
            }
            foreach (Control item in groupBox2.Controls)
            {
                if (item is TextBox)
                {
                    TextBox text = (TextBox)item;
                    text.Clear();
                }
            }
            foreach (Control item in groupBox6.Controls)
            {
                if (item is TextBox)
                {
                    TextBox text = (TextBox)item;
                    text.Clear();
                }
            }
            foreach (Control item in groupBox5.Controls)
            {
                if (item is TextBox)
                {
                    TextBox text = (TextBox)item;
                    text.Clear();
                }
            }
            foreach (Control item in groupBox14.Controls)
            {
                if (item is TextBox)
                {
                    TextBox text = (TextBox)item;
                    text.Clear();
                }
            }
            foreach (Control item in groupBox15.Controls)
            {
                if (item is TextBox)
                {
                    TextBox text = (TextBox)item;
                    text.Clear();
                }
            }
            foreach (Control item in groupBox17.Controls)
            {
                if (item is TextBox)
                {
                    TextBox text = (TextBox)item;
                    text.Clear();
                }
            }

        }

        private void ClearSingle()
        {
            lblSingleIncome.Text = "";
            lblSingleOutcome.Text = "";
            lblSingleRevenue.Text = "";
        }

        private void ClearMultiple()
        {
            lblMultipleIncome.Text = "";
            lblMultipleOutcome.Text = "";
            lblMultipleRevenue.Text = "";
        }

        private void ClearMonthly()
        {
            lblMonthlyInc.Text = "";
            lblMonthlyOut.Text = "";
            lblMonthlyRevenue.Text = "";
        }

        private void ClearYearly()
        {
            lblYearlyInc.Text = "";
            lblYearlyOut.Text = "";
            lblYearlyRevenue.Text = "";
        }

        private void FillComboBoxes()
        {
            List<Employees> emps = db.Employees.Where(x => x.Status == true).ToList();
            List<PaymentTypes> pts = db.PaymentTypes.Where(x => x.Status == true).ToList();

            cmbAddEmployees.DataSource = emps;
            cmbAddEmployees.ValueMember = "ID";
            cmbAddEmployees.DisplayMember = "Name";

            cmbAddPaymentType.DataSource = pts;
            cmbAddPaymentType.ValueMember = "ID";
            cmbAddPaymentType.DisplayMember = "Type";

            cmbGetEmpForReport.DataSource = emps;
            cmbGetEmpForReport.ValueMember = "ID";
            cmbGetEmpForReport.DisplayMember = "Name";

            cmbAddEmployees.SelectedIndex = -1;
            cmbAddPaymentType.SelectedIndex = -1;
            cmbGetEmpForReport.SelectedIndex = -1;


            cmbAddEmployees.DropDownHeight = cmbAddEmployees.ItemHeight * 5;
            cmbAddPaymentType.DropDownHeight = cmbAddEmployees.ItemHeight * 5;
            cmbGetEmpForReport.DropDownHeight = cmbGetEmpForReport.ItemHeight * 5;
        }

        private void RefreshComboBoxes()
        {
            if (cmbAddEmployees.Items.Count > 0 || cmbAddPaymentType.Items.Count > 0 || cmbGetEmpForReport.Items.Count > 0)
            {
                cmbAddEmployees.DataSource = null;
                cmbAddPaymentType.DataSource = null;
                cmbGetEmpForReport.DataSource = null;

                FillComboBoxes();
            }
        }


        ////////////////// Genel Metotlar Bitiş ////////////////////



        ////////////////////////////// Raporlama Başlangıç //////////////////////
        private void dtpSingleSelect_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (dtpSingleSelect.Value > DateTime.Now)
                {
                    MetroMessageBox.Show(this, "İleri bir tarih seçemezsiniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    ClearSingle();
                    return;
                }
                var SelectedDate = dtpSingleSelect.Value.ToString("yyyy-MM-dd");
                var formattedDate = Convert.ToDateTime(SelectedDate);

                List<Incomes> incomes = db.Incomes.Where(x => x.Date == formattedDate && x.Status == true).ToList();
                List<Outcomes> outcomes = db.Outcomes.Where(x => x.Date == formattedDate && x.Status == true).ToList();

                lblSingleIncome.Text = string.Format("{0:c}", incomes.Sum(x => x.Amount));
                lblSingleOutcome.Text = string.Format("{0:c}", outcomes.Sum(x => x.Amount));

                var revenue = incomes.Sum(x => x.Amount) - outcomes.Sum(x => x.Amount);
                if (revenue > 0)
                {
                    lblSingleRevenue.ForeColor = Color.Green;
                    lblSingleRevenue.Text = string.Format("{0:c}", revenue);
                }
                else if (revenue == 0 || revenue <= 0)
                {
                    lblSingleRevenue.ForeColor = Color.Red;
                    lblSingleRevenue.Text = string.Format("{0:c}", revenue);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message); ;
            }
        }

        private void btnGetMultiple_Click(object sender, EventArgs e)
        {
            try
            {
                if (dtpMultiSelectSecond.Value > DateTime.Now)
                {
                    MetroMessageBox.Show(this, "İleri bir tarih seçemezsiniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    ClearMultiple();
                    return;
                }
                var SelectedDate = dtpMultiSelectFirst.Value.ToString("yyyy-MM-dd");
                var SelectedSecondDate = dtpMultiSelectSecond.Value.ToString("yyyy-MM-dd");

                var formattedDate = Convert.ToDateTime(SelectedDate);
                var formattedSecondDate = Convert.ToDateTime(SelectedSecondDate);

                List<Incomes> incomes = db.Incomes.Where(x => x.Status == true).Where(x => x.Date >= formattedDate).Where(x => x.Date <= formattedSecondDate).ToList();
                List<Outcomes> outcomes = db.Outcomes.Where(x => x.Status == true).Where(x => x.Date >= formattedDate).Where(x => x.Date <= formattedSecondDate).ToList();


                lblMultipleIncome.Text = string.Format("{0:c}", incomes.Sum(x => x.Amount));
                lblMultipleOutcome.Text = string.Format("{0:c}", outcomes.Sum(x => x.Amount));

                var revenue = incomes.Sum(x => x.Amount) - outcomes.Sum(x => x.Amount);
                if (revenue > 0)
                {
                    lblMultipleRevenue.ForeColor = Color.Green;
                    lblMultipleRevenue.Text = string.Format("{0:c}", revenue);
                }
                else if (revenue == 0 || revenue <= 0)
                {
                    lblMultipleRevenue.ForeColor = Color.Red;
                    lblMultipleRevenue.Text = string.Format("{0:c}", revenue);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void btnGetMonthlyReport_Click(object sender, EventArgs e)
        {
            try
            {
                var selectedMonth = dtpMonthlyReport.Value.Month;
                var selectedYear = dtpMonthlyReportYear.Value.Year;

                if (selectedYear > DateTime.Now.Year)
                {
                    MetroMessageBox.Show(this, "İleri bir tarih seçemezsiniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    ClearMonthly();
                    return;
                }
                if (selectedMonth > DateTime.Now.Month)
                {
                    MetroMessageBox.Show(this, "İleri bir tarih seçemezsiniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    ClearMonthly();
                    return;
                }


                List<Incomes> incomes = db.Incomes.Where(x => x.Status == true).Where(x => x.Date.Value.Month == selectedMonth).Where(x => x.Date.Value.Year == selectedYear).ToList();

                List<Outcomes> outcomes = db.Outcomes.Where(x => x.Status == true).Where(x => x.Date.Value.Month == selectedMonth).Where(x => x.Date.Value.Year == selectedYear).ToList();

                lblMonthlyInc.Text = string.Format("{0:c}", incomes.Sum(x => x.Amount));
                lblMonthlyOut.Text = string.Format("{0:c}", outcomes.Sum(x => x.Amount));

                var revenue = incomes.Sum(x => x.Amount) - outcomes.Sum(x => x.Amount);
                if (revenue > 0)
                {
                    lblMonthlyRevenue.ForeColor = Color.Green;
                    lblMonthlyRevenue.Text = string.Format("{0:c}", revenue);
                }
                else if (revenue == 0 || revenue <= 0)
                {
                    lblMonthlyRevenue.ForeColor = Color.Red;
                    lblMonthlyRevenue.Text = string.Format("{0:c}", revenue);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void btnGetYearlyReport_Click(object sender, EventArgs e)
        {
            try
            {
                var selectedYear = dtpYearlyReport.Value.Year;

                if (selectedYear > DateTime.Now.Year)
                {
                    MetroMessageBox.Show(this, "İleri bir tarih seçemezsiniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    ClearYearly();
                    return;
                }

                List<Incomes> incomes = db.Incomes.Where(x => x.Status == true).Where(x => x.Date.Value.Year == selectedYear).ToList();

                List<Outcomes> outcomes = db.Outcomes.Where(x => x.Status == true).Where(x => x.Date.Value.Year == selectedYear).ToList();

                lblYearlyInc.Text = string.Format("{0:c}", incomes.Sum(x => x.Amount));
                lblYearlyOut.Text = string.Format("{0:c}", outcomes.Sum(x => x.Amount));

                var revenue = incomes.Sum(x => x.Amount) - outcomes.Sum(x => x.Amount);
                if (revenue > 0)
                {
                    lblYearlyRevenue.ForeColor = Color.Green;
                    lblYearlyRevenue.Text = string.Format("{0:c}", revenue);
                }
                else if (revenue == 0 || revenue <= 0)
                {
                    lblYearlyRevenue.ForeColor = Color.Red;
                    lblYearlyRevenue.Text = string.Format("{0:c}", revenue);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        ////////////////////////////// Raporlama Bitiş //////////////////////


        ///////////////////////////// Grafik Başlangıç //////////////////////

        private void FillChart()
        {
            foreach (Control item in groupBox12.Controls)
            {
                if (item is Chart)
                {
                    groupBox12.Controls.Remove(item);
                    break;
                }
            }

            Chart ch = new Chart();
            ch.Location = new Point(13, 28);
            ch.Size = new Size(845, 365);
            ch.ChartAreas.Add(new ChartArea("Gelir"));
            ch.Series.Add(new Series("Gelir"));
            ch.Legends.Add(new Legend("Gelir"));
            groupBox12.Controls.Add(ch);

            #region Months
            Dictionary<string, int> months = new Dictionary<string, int>();
            months.Add("Ocak", 1);
            months.Add("Şubat", 2);
            months.Add("Mart", 3);
            months.Add("Nisan", 4);
            months.Add("Mayıs", 5);
            months.Add("Haziran", 6);
            months.Add("Temmuz", 7);
            months.Add("Ağustos", 8);
            months.Add("Eylül", 9);
            months.Add("Ekim", 10);
            months.Add("Kasım", 11);
            months.Add("Aralık", 12);
            #endregion

            List<Incomes> incomeList = new List<Incomes>();
            using (dcDBEntities dd = new dcDBEntities())
            {
                incomeList = dd.Incomes.Where(x => x.Status == true && x.Date.Value.Year == DateTime.Now.Year).ToList();
            }


            ch.ChartAreas[0].AxisX.Interval = 1;

            foreach (KeyValuePair<string, int> month in months)
            {
                ch.Series["Gelir"].Points.AddXY(month.Key, incomeList.Where(x => x.Date.Value.Month == month.Value).Sum(x => x.Amount));
            }

            ch.Series["Gelir"].Label = "#VALY{C2}";
            ch.Series["Gelir"].LabelBackColor = Color.Black;
            ch.Series["Gelir"].LabelForeColor = Color.White;
            ch.Series["Gelir"].Font = new Font(new FontFamily("Segoe UI"), 8, FontStyle.Bold);


            ch.Titles.Add("Yıllık Gelirin Aylara Göre Dağılımı");
        }

        private void FillYearlyOutcome()
        {
            foreach (Control item in groupBox11.Controls)
            {
                if (item is Chart)
                {
                    groupBox11.Controls.Remove(item);
                    break;
                }
            }

            Chart ch = new Chart();
            ch.Location = new Point(13, 28);
            ch.Size = new Size(845, 365);
            ch.ChartAreas.Add(new ChartArea("Gider"));
            ch.Series.Add(new Series("Gider"));
            ch.Legends.Add(new Legend("Gider"));
            groupBox11.Controls.Add(ch);



            #region Months
            Dictionary<string, int> months = new Dictionary<string, int>();
            months.Add("Ocak", 1);
            months.Add("Şubat", 2);
            months.Add("Mart", 3);
            months.Add("Nisan", 4);
            months.Add("Mayıs", 5);
            months.Add("Haziran", 6);
            months.Add("Temmuz", 7);
            months.Add("Ağustos", 8);
            months.Add("Eylül", 9);
            months.Add("Ekim", 10);
            months.Add("Kasım", 11);
            months.Add("Aralık", 12);
            #endregion

            List<Outcomes> outcomeList = new List<Outcomes>();
            using (dcDBEntities dd = new dcDBEntities())
            {
                outcomeList = dd.Outcomes.Where(x => x.Status == true && x.Date.Value.Year == DateTime.Now.Year).ToList();
            }


            ch.ChartAreas[0].AxisX.Interval = 1;

            foreach (KeyValuePair<string, int> month in months)
            {
                ch.Series["Gider"].Points.AddXY(month.Key, outcomeList.Where(x => x.Date.Value.Month == month.Value).Sum(x => x.Amount));
            }
            ch.Series["Gider"].Label = "#VALY{C2}";
            ch.Series["Gider"].LabelBackColor = Color.Black;
            ch.Series["Gider"].LabelForeColor = Color.White;
            ch.Series["Gider"].Font = new Font(new FontFamily("Segoe UI"), 8, FontStyle.Bold);

            ch.Titles.Add("Yıllık Giderin Aylara Göre Dağılımı");
        }

        private void FillYearlyRevenue()
        {
            foreach (Control item in groupBox13.Controls)
            {
                if (item is Chart)
                {
                    groupBox13.Controls.Remove(item);
                    break;
                }
            }

            Chart ch = new Chart();
            ch.Location = new Point(13, 28);
            ch.Size = new Size(845, 365);
            ch.ChartAreas.Add(new ChartArea("Toplam"));
            ch.Series.Add(new Series("Toplam"));
            ch.Legends.Add(new Legend("Toplam"));
            groupBox13.Controls.Add(ch);


            #region Months
            Dictionary<string, int> months = new Dictionary<string, int>();
            months.Add("Ocak", 1);
            months.Add("Şubat", 2);
            months.Add("Mart", 3);
            months.Add("Nisan", 4);
            months.Add("Mayıs", 5);
            months.Add("Haziran", 6);
            months.Add("Temmuz", 7);
            months.Add("Ağustos", 8);
            months.Add("Eylül", 9);
            months.Add("Ekim", 10);
            months.Add("Kasım", 11);
            months.Add("Aralık", 12);
            #endregion

            List<Incomes> incomeList = new List<Incomes>();
            List<Outcomes> outcomeList = new List<Outcomes>();
            using (dcDBEntities dd = new dcDBEntities())
            {
                outcomeList = dd.Outcomes.Where(x => x.Status == true && x.Date.Value.Year == DateTime.Now.Year).ToList();
                incomeList = db.Incomes.Where(x => x.Status == true && x.Date.Value.Year == DateTime.Now.Year).ToList();
            }



            ch.ChartAreas[0].AxisX.Interval = 1;

            foreach (KeyValuePair<string, int> month in months)
            {
                ch.Series["Toplam"].Points.AddXY(month.Key, incomeList.Where(x => x.Date.Value.Month == month.Value).Sum(x => x.Amount) - outcomeList.Where(x => x.Date.Value.Month == month.Value).Sum(x => x.Amount));
            }
            ch.Series["Toplam"].Label = "#VALY{C2}";
            ch.Series["Toplam"].LabelBackColor = Color.Black;
            ch.Series["Toplam"].LabelForeColor = Color.White;
            ch.Series["Toplam"].Font = new Font(new FontFamily("Segoe UI"), 8, FontStyle.Bold);

            ch.Titles.Add("Yıllık Karın Aylara Göre Dağılımı");
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            FillChart();
        }

        private void btnRefreshOutcome_Click(object sender, EventArgs e)
        {
            FillYearlyOutcome();
        }

        private void btnRefreshRevenue_Click(object sender, EventArgs e)
        {
            FillYearlyRevenue();
        }
        ///////////////////////////// Grafik Bitiş //////////////////////


        /////////////////// Personel İşlemleri Başlangıç  ///////////////
        private void btnAddEmp_Click(object sender, EventArgs e)
        {
            Employees emp = new Employees();
            if (string.IsNullOrWhiteSpace(txtAddEmpName.Text) || string.IsNullOrWhiteSpace(txtAddEmpBranch.Text) || string.IsNullOrWhiteSpace(txtAddEmpSalary.Text))
            {
                MetroMessageBox.Show(this, "Alanlar boş geçilemez !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            try
            {
                emp.Name = txtAddEmpName.Text.ToUpper();
                emp.Branch = txtAddEmpBranch.Text;
                emp.Salary = Convert.ToDecimal(txtAddEmpSalary.Text);
                emp.CreatedDate = DateTime.Now;
                emp.Status = true;
                db.Employees.Add(emp);
                db.SaveChanges();
                Clear();
                FillListViewEmployees();
                UpdateTotalEmployees();
                RefreshComboBoxes();
                MetroMessageBox.Show(this, "Kayıt başarıyla eklendi !", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateTotalEmployees()
        {
            string empCount = db.Employees.Where(x => x.Status == true).Count().ToString();

            lblEmpCount.Text = empCount;
        }

        private void btnUpdateEmp_Click(object sender, EventArgs e)
        {
            try
            {
                if (lvEmpList.SelectedItems.Count == 0)
                {
                    MetroMessageBox.Show(this, "Kayıt Seçmediniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
                int id = Convert.ToInt32(lblEmpID.Text);
                Employees emp = db.Employees.FirstOrDefault(x => x.ID == id);
                if (emp != null)
                {
                    DialogResult dr = MetroMessageBox.Show(this, "Seçili kaydı güncellemek istediğinizden emin misiniz ?", "Uyarı", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                    if (dr == DialogResult.Yes)
                    {
                        emp.Name = txtChangeEmpName.Text.ToUpper();
                        emp.Branch = txtChangeEmpBranch.Text;
                        emp.Salary = Convert.ToDecimal(txtChangeEmpSalary.Text);
                        db.SaveChanges();
                        FillListViewEmployees();
                        Clear();
                        UpdateTotalEmployees();
                        lblEmpID.Text = "";
                        MetroMessageBox.Show(this, "Güncelleme İşlemi Başarılı !", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else if (dr == DialogResult.No)
                    {
                        MetroMessageBox.Show(this, "Güncelleme işlemi iptal edildi !", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        Clear();
                        FillListViewEmployees();
                        lblEmpID.Text = "";
                    }
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void lvEmpList_Click(object sender, EventArgs e)
        {
            lblEmpID.Text = lvEmpList.SelectedItems[0].Tag.ToString();
            int id = Convert.ToInt32(lblEmpID.Text);
            Employees emp = db.Employees.FirstOrDefault(x => x.ID == id);

            txtChangeEmpName.Text = emp.Name;
            txtChangeEmpBranch.Text = emp.Branch;
            txtChangeEmpSalary.Text = string.Format("{0:f2}", emp.Salary);
        }

        private void btnDeleteEmp_Click(object sender, EventArgs e)
        {
            try
            {
                if (lvEmpList.SelectedItems.Count == 0)
                {
                    MetroMessageBox.Show(this, "Kayıt Seçmediniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
                int id = Convert.ToInt32(lblEmpID.Text);
                Employees emp = db.Employees.FirstOrDefault(x => x.ID == id);
                if (emp != null)
                {
                    DialogResult dr = MetroMessageBox.Show(this, "Seçili kaydı silmek istediğinizden emin misiniz ?", "Uyarı", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                    if (dr == DialogResult.Yes)
                    {
                        emp.Status = false;
                        db.SaveChanges();
                        FillListViewEmployees();
                        Clear();
                        UpdateTotalEmployees();
                        RefreshComboBoxes();
                        lblEmpID.Text = "";
                        MetroMessageBox.Show(this, "Silme İşlemi Başarılı !", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else if (dr == DialogResult.No)
                    {
                        MetroMessageBox.Show(this, "Silme işlemi iptal edildi !", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        Clear();
                        FillListViewEmployees();
                        lblEmpID.Text = "";
                    }
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        /////////// Ödeme İşlemleri Başlangıç //////////////////
        private void btnAddPayment_Click(object sender, EventArgs e)
        {
            if (cmbAddEmployees.SelectedItem == null || cmbAddPaymentType.SelectedItem == null)
            {
                MetroMessageBox.Show(this, "Kayıt seçmediniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtAddPaymentAmount.Text))
            {
                MetroMessageBox.Show(this, "Miktar girmediniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if (dtpAddPaymentDate.Value > DateTime.Now)
            {
                MetroMessageBox.Show(this, "İleri tarihli kayıt giremezsiniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            int selectedEmpID = Convert.ToInt32(cmbAddEmployees.SelectedValue);
            int selectedPaymentID = Convert.ToInt32(cmbAddPaymentType.SelectedValue);

            Employees emp = db.Employees.FirstOrDefault(x => x.ID == selectedEmpID);
            PaymentTypes pts = db.PaymentTypes.FirstOrDefault(x => x.ID == selectedPaymentID);

            if (emp != null && pts != null)
            {
                try
                {
                    Payments payment = new Payments();
                    payment.EmployeeID = emp.ID;
                    payment.PaymentTypeID = pts.ID;
                    payment.Amount = Convert.ToDecimal(txtAddPaymentAmount.Text);
                    payment.Date = dtpAddPaymentDate.Value;
                    payment.Status = true;

                    db.Payments.Add(payment);
                    db.SaveChanges();
                    Clear();
                    RefreshComboBoxes();
                    FillListViewPayments();
                    lblPaymentID.Text = "";
                    MetroMessageBox.Show(this, "Kayıt başarıyla eklendi !", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void lwPayments_Click(object sender, EventArgs e)
        {
            lblPaymentID.Text = lwPayments.SelectedItems[0].Tag.ToString();
            int id = Convert.ToInt32(lblPaymentID.Text);
            Payments payment = db.Payments.FirstOrDefault(x => x.ID == id);
        }

        private void btnDeletePayment_Click(object sender, EventArgs e)
        {
            try
            {
                if (lwPayments.SelectedItems.Count == 0)
                {
                    MetroMessageBox.Show(this, "Kayıt Seçmediniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
                int id = Convert.ToInt32(lblPaymentID.Text);
                Payments payments = db.Payments.FirstOrDefault(x => x.ID == id);
                if (payments != null)
                {
                    DialogResult dr = MetroMessageBox.Show(this, "Seçili kaydı silmek istediğinizden emin misiniz ?", "Uyarı", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                    if (dr == DialogResult.Yes)
                    {
                        payments.Status = false;
                        db.SaveChanges();
                        FillListViewPayments();
                        lblPaymentID.Text = "";
                        MetroMessageBox.Show(this, "Silme İşlemi Başarılı !", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else if (dr == DialogResult.No)
                    {
                        MetroMessageBox.Show(this, "Silme işlemi iptal edildi !", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        lblPaymentID.Text = "";
                    }
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void btnGetPaymentReport_Click(object sender, EventArgs e)
        {
            try
            {
                int selectedMonth = dtpPaymentMonth.Value.Month;
                int selectedYear = dtpPaymentYear.Value.Year;
                int selectedEmployeeID = Convert.ToInt32(cmbGetEmpForReport.SelectedValue);

                if (selectedYear > DateTime.Now.Year)
                {
                    MetroMessageBox.Show(this, "İleri bir tarih seçemezsiniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
                if (selectedMonth > DateTime.Now.Month)
                {
                    MetroMessageBox.Show(this, "İleri bir tarih seçemezsiniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
                if (cmbGetEmpForReport.SelectedItem == null)
                {
                    MetroMessageBox.Show(this, "Personel seçmediniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                Employees employee = db.Employees.FirstOrDefault(x => x.Status == true && x.ID == selectedEmployeeID);

                List<Payments> payments = db.Payments.Where(x => x.Status == true).Where(x => x.Date.Value.Month == selectedMonth).Where(x => x.Date.Value.Year == selectedYear).Where(x => x.EmployeeID == selectedEmployeeID).ToList();

                List<PaymentTypes> paymentTypes = db.PaymentTypes.ToList();

                lblReportSalary.Text = string.Format("{0:c}", employee.Salary);

                if (payments.Count > 0)
                {
                    lblReportBank.Text = string.Format("{0:c}", payments.Where(x => x.PaymentTypeID == 1).Sum(x => x.Amount));
                    lblReportAvans.Text = string.Format("{0:c}", payments.Where(x => x.PaymentTypeID == 2).Sum(x => x.Amount));
                    lblReportElden.Text = string.Format("{0:c}", payments.Where(x => x.PaymentTypeID == 3).Sum(x => x.Amount));

                    decimal kalan = (decimal)(payments.Where(x => x.PaymentTypeID == 1).Sum(x => x.Amount) + payments.Where(x => x.PaymentTypeID == 2).Sum(x => x.Amount) + payments.Where(x => x.PaymentTypeID == 3).Sum(x => x.Amount));

                    lblReportKalan.ForeColor = Color.Red;
                    lblReportKalan.Text = string.Format("{0:c}", employee.Salary - kalan);

                }
                else
                {
                    MetroMessageBox.Show(this, "Kişiye bu ay hiç ödeme yapılmamış !", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void onlyDigit_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && e.KeyChar != ',';
        }

        /////////// Ödeme İşlemleri Bitiş //////////////////
        ///////////////////   Personel İşlemleri Bitiş   ////////////////



        //////////////////  CamLogs Başlangıç ////////////////////
        public static ArrayList RawFileNames = new ArrayList();

        private void lvNewPhotos_Click(object sender, EventArgs e)
        {
            if (!bgw.IsBusy)
            {
                bgw.RunWorkerAsync();
            }
        }

        private void Bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string fileName = lvNewPhotos.SelectedItems[0].Tag.ToString();

                CamLogs selectedLog = db.CamLogs.FirstOrDefault(x => x.Name == fileName);
                string selectedLogName = selectedLog.Name;


                string filepath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DCIO" + "\\";

                if (RawFileNames.Count == 0)
                {
                    RawFileNames = new ArrayList(Directory.GetFiles(filepath));
                }
                int counter = 0;
                foreach (string item in RawFileNames)
                {
                    if (item.ToLower().Contains(selectedLogName.ToLower()))
                    {
                        pbNewPhoto.Image = Bitmap.FromFile(Path.Combine(filepath, selectedLogName));
                        counter += 1;
                        selectedLog.isSeen = true;
                        db.SaveChanges();
                        int notCount = NotificationCount();
                        if (notCount == 0)
                        {
                            notTimer.Stop();
                        }
                    }
                }
                if (counter == 0)
                {
                    MetroMessageBox.Show(this, "Seçtiğiniz kayda ait görüntü silinmiş !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    selectedLog.Status = false;
                    db.SaveChanges();
                    FillListViewNewPhotos();
                    int notCount = NotificationCount();
                    if (notCount == 0)
                    {
                        notTimer.Stop();
                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }


        private int NotificationCount()
        {
            int notificationCounter = db.CamLogs.Where(x => x.isSeen == false && x.Status == true).Count();
            if (notificationCounter == 0)
            {
                lblNoficationCounter.Visible = false;
            }
            else if (notificationCounter > 0)
            {
                lblNoficationCounter.Text = notificationCounter.ToString();
            }
            return notificationCounter;
        }

        private void metroTabControl4_Click(object sender, EventArgs e)
        {
            FillListViewNewPhotos();
            FillListViewOldPhotos();
            pbNewPhoto.Image = null;
            pbOldPhoto.Image = null;
        }


        public static ArrayList OldRawFileNames = new ArrayList();
        private void lvOldPhotos_Click(object sender, EventArgs e)
        {
            try
            {
                string fileName = lvOldPhotos.SelectedItems[0].Tag.ToString();

                CamLogs selectedLog = db.CamLogs.FirstOrDefault(x => x.Name == fileName);
                string selectedLogName = selectedLog.Name;


                string filepath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DCIO" + "\\";

                if (OldRawFileNames.Count == 0)
                {
                    OldRawFileNames = new ArrayList(Directory.GetFiles(filepath));
                }
                int counter = 0;
                foreach (string item in OldRawFileNames)
                {
                    if (item.ToLower().Contains(selectedLogName.ToLower()))
                    {
                        pbOldPhoto.Image = Bitmap.FromFile(Path.Combine(filepath, selectedLogName));
                        counter += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }




        private int DeleteImageFromFolder(string name)
        {
            int isSuccess = 0;
            ArrayList DeletedFileNames = new ArrayList();
            try
            {
                string filepath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DCIO" + "\\";

                if (DeletedFileNames.Count == 0)
                {
                    DeletedFileNames = new ArrayList(Directory.GetFiles(filepath));
                }
                foreach (string item in DeletedFileNames)
                {
                    if (item.ToLower().Contains(name.ToLower()))
                    {
                        if (File.Exists(item))
                        {
                            System.GC.Collect();
                            System.GC.WaitForPendingFinalizers();
                            File.Delete(item);
                            isSuccess = 1;
                        }
                        else
                        {
                            isSuccess = -1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                isSuccess = 3;
            }
            return isSuccess;
        }


        private void btnDeletePhoto_Click(object sender, EventArgs e)
        {
            try
            {
                if (lvOldPhotos.SelectedItems.Count == 0)
                {
                    MetroMessageBox.Show(this, "Kayıt Seçmediniz !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
                string fileName = lvOldPhotos.SelectedItems[0].Tag.ToString();

                CamLogs selectedLog = db.CamLogs.FirstOrDefault(x => x.Name == fileName);
                string selectedLogName = selectedLog.Name;

                if (selectedLog != null)
                {
                    DialogResult dr = MetroMessageBox.Show(this, "Seçili kaydı silmek istediğinizden emin misiniz ?  Daha sonra geri GETİREMEZSİNİZ !!", "Uyarı", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                    if (dr == DialogResult.Yes)
                    {
                        selectedLog.Status = false;
                        db.SaveChanges();
                        pbOldPhoto.Image = null;
                        FillListViewOldPhotos();
                        if (DeleteImageFromFolder(selectedLogName) == 1)
                        {
                            MetroMessageBox.Show(this, "Silme işlemi Başarılı !", "Başarılı !", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                        else if (DeleteImageFromFolder(selectedLogName) == 2)
                        {
                            MetroMessageBox.Show(this, "Silmeye çalıştığınız dosya daha önce elle silinmiş olabilir!", "Başarısız !", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                        else if (DeleteImageFromFolder(selectedLogName) == 3)
                        {
                            MetroMessageBox.Show(this, "Beklenmedik bir hata oluştu ! Bunu kaydettik ve inceleyeceğiz..", "Uyarı !", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                    }
                    else if (dr == DialogResult.No)
                    {
                        MetroMessageBox.Show(this, "Silme işlemi iptal edildi !", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        //////////////////  CamLogs Bitiş  ////////////////////


        private void btnBackupDB_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.ShowDialog();
                string filepath = fbd.SelectedPath;

                string dbName = ConfigurationManager.AppSettings["dbName"];
                string ctor = ConfigurationManager.AppSettings["conStr"];

                SqlConnection con = new SqlConnection(ctor);
                string ticks = DateTime.Now.Ticks.ToString();
                string query = @" declare @DBFileName varchar(256) " +
"set @DBFileName = '" + filepath + "' + datename(dd, getdate()) + " +
"datename(m, getdate()) + datename(yy, getdate()) +''+ datename(HH, getdate()) + ''+ datename(MI, getdate())+ '_" + ticks + "_" + dbName + ".bak' " +
"select @DBFileName " +
"BACKUP DATABASE " + dbName + @" TO  DISK = @DBFileName " +
"WITH RETAINDAYS = 30, NAME = N'" + dbName + "Backup', SKIP";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                int ct = cmd.ExecuteNonQuery();
                con.Close();
                MetroMessageBox.Show(this, "Veritabanı yedekleme başarılı !", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception)
            {
                MetroMessageBox.Show(this, "Veritabanı yedeklenirken bir hata meydana geldi!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void FillSettinsTextBoxes()
        {
            try
            {
                MailSettings ms = db.MailSettings.FirstOrDefault(x => x.ID == 1);
                txtSMTP.Text = ms.SmptServer;
                txtMailAdd.Text = ms.MailAddress;
                txtMailPass.Text = ms.MailPassword;
                txtMailTo.Text = ms.MailTo;
            }
            catch (Exception)
            {

            }
        }
    }
}
