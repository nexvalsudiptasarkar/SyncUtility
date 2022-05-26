using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nexval.NexAEI.Utility.UI
{
    public partial class frmClassic : Form
    {
        public frmClassic()
        {
            InitializeComponent();
            SetupForm();

        }
        private void SetupForm()
        {
            dtpStartDate.Value = DateTime.Now.AddDays(-1);
            if(ConfigurationManager.AppSettings["EnableTrackerUpdate"] =="0")
            {
                chkTracker.Visible = false;
            }
        }

        private void dtpEndDate_ValueChanged(object sender, EventArgs e)
        {
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            statusStrip1.Show();
            toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
            timer1.Enabled = true;
            timer1.Start();
            var param = new WorkerParam
            {
                IfTrackerEnabled = chkTracker.Checked,
                MaxProgressSize = 2000
            };
            backgroundWorker1.RunWorkerAsync(param);

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker helperBW = sender as BackgroundWorker;
            ShowStatus("Connecting...");
            var arg = (WorkerParam)e.Argument;
            e.Result = BackgroundProcessLogicMethod(helperBW, arg);

            if (helperBW.CancellationPending)
            {
                e.Cancel = true;
            }
        }
        private int BackgroundProcessLogicMethod(BackgroundWorker bw, WorkerParam param)
        {
            ShowStatus("Progressing..."); 
            int result = 0;
            SendAttendance(param);            
            return result;
        }
        private void SendAttendance(WorkerParam param)
        {
            System.Threading.Thread.Sleep(2000);
            var configInfo = new ConfigurationInfo();
            try
            {

                ProcessManager manager = new ProcessManager();
                // configInfo = manager.GetTrackerValues(configInfo); 
                configInfo.StartDateTime = dtpStartDate.Value;
                configInfo.LastDateTime = dtpEndDate.Value;
                var res = manager.ExportDataToNexAei(configInfo);
                param.StatusMessage = res;
                if (param.IfTrackerEnabled)
                {
                    ShowStatus("Updateing tracker...");
                    DataAccess.UpdateTracker(configInfo);
                }
                ShowStatus(res);
                MessageBox.Show(res);
                ShowResults(res);
            }
            catch (Exception ex)
            {

                Utility.LogMessage(configInfo, ex.Message, "Main");

            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled) MessageBox.Show("Operation was canceled");
            else if (e.Error != null) MessageBox.Show(e.Error.Message);
            else ShowStatus("Completed");

            timer1.Stop();
            timer1.Enabled = false;
            toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
        }
       
        private void ShowStatus(string text)
        {
            if (statusStrip1.InvokeRequired)
            {
                
                this.statusStrip1.Invoke(new Action<string>(n => toolStripStatusLabel1.Text = n), new object[] { text });

            }
            else
            {
                toolStripStatusLabel1.Text = text;
            }
        }

        private void ShowResults(string text)
        {
            if (lblMsg.InvokeRequired)
            {

                this.lblMsg.Invoke(new Action<string>(n => lblMsg.Text = n), new object[] { text });

            }
            else
            {
                lblMsg.Text = text;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (toolStripProgressBar1.Value == toolStripProgressBar1.Maximum)
            {
                toolStripProgressBar1.Value = 0;
            }
            toolStripProgressBar1.Value += 1;
        }

        private void dtpEndDate_MouseCaptureChanged(object sender, EventArgs e)
        {

            if (dtpEndDate.Value > DateTime.Now)
            {
                MessageBox.Show("End Date should not be later than today.");
                dtpEndDate.Value = DateTime.Now;
                return;
            }
            if (dtpEndDate.Value < dtpStartDate.Value)
            {
                MessageBox.Show("End Date should not be later than start date.");
                dtpEndDate.Value = DateTime.Now;
                return;
            }

        }
    }
}
