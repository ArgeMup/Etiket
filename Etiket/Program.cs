using ArgeMup.HazirKod.Ekİşlemler;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Etiket
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [System.STAThread]
        static void Main(string[] BaşlangıçParamaetreleri)
        {
#if DEBUG
            BaşlangıçParamaetreleri = new string[] { "5555" };
#endif

            if (BaşlangıçParamaetreleri == null ||
                BaşlangıçParamaetreleri.Length != 1 ||
                !int.TryParse(BaşlangıçParamaetreleri[0], out int ŞubeErişimNoktası) ||
                ŞubeErişimNoktası < 0)
            {
                ŞubeErişimNoktası = 0;
            }

            Application.ThreadException += new ThreadExceptionEventHandler(BeklenmeyenDurum_KullanıcıArayüzü);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(BeklenmeyenDurum_Uygulama);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AnaKontrolcü.Açıl(ŞubeErişimNoktası);

            Application.Run(AnaKontrolcü.BoştaBekleyenAnaUygulama);
            AnaKontrolcü.Kapan("Normal");
        }

        static void BeklenmeyenDurum_Uygulama(object sender, UnhandledExceptionEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException -= BeklenmeyenDurum_Uygulama;
            BeklenmeyenDurum((Exception)e.ExceptionObject);
        }
        static void BeklenmeyenDurum_KullanıcıArayüzü(object sender, ThreadExceptionEventArgs t)
        {
            Application.ThreadException -= BeklenmeyenDurum_KullanıcıArayüzü;
            BeklenmeyenDurum(t.Exception);
        }
        static void BeklenmeyenDurum(Exception ex)
        {
            ex.Günlük(Hemen: true);

            try
            {
                string hata = "Bir sorun oluştu, uygulama kapatılacak." + Environment.NewLine + Environment.NewLine + ex.Message;

                AnaKontrolcü.Kapan("BeklenmeyenDurum");

                MessageBox.Show(hata, "Barkod Üret");
            }
            catch (Exception exxx)
            {
                exxx.Günlük(Hemen: true);

                MessageBox.Show(ex.Message + Environment.NewLine + exxx.Message, "Barkod Üret");
            }
            finally
            {
                Application.Exit();
            }
        }
    }
}
