using ArgeMup.HazirKod;
using System.IO;

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
            Depo_ KomutVermeDeposu = new Depo_();
            KomutVermeDeposu["Ayarlar", 0] = @"C:\Deneme\Ayarlar.mup";

            ////Seçenek 1
            //KomutVermeDeposu["Komut", 0] = "Ayarla";
            //KomutVermeDeposu["Ön Tanımlı Görseller/Sabit 1"].İçeriği = new string[] { true.ToString(), "_?? Yazı" };
            //KomutVermeDeposu["Ön Tanımlı Görseller/Sabit 2"].İçeriği = new string[] { false.ToString(), "_?? Resim" };
            //BaşlangıçParamaetreleri = new string[] { @"C:\Deneme\Komut.txt" };
            //File.WriteAllText(BaşlangıçParamaetreleri[0], KomutVermeDeposu.YazıyaDönüştür());

            ////Seçenek 2 A
            //KomutVermeDeposu["Komut", 0] = "Ayarla";
            //KomutVermeDeposu["Ön Tanımlı Görseller/Sabit 1"].İçeriği = new string[] { true.ToString(), "_?? Yazı" };
            //KomutVermeDeposu["Ön Tanımlı Görseller/Sabit 2"].İçeriği = new string[] { false.ToString(), "_?? Resim" };
            //BaşlangıçParamaetreleri = new string[] { ArgeMup.HazirKod.Dönüştürme.D_Yazı.Taban64e(KomutVermeDeposu.YazıyaDönüştür()) };

            ////Seçenek 2 B
            //KomutVermeDeposu["Komut", 0] = "Yazdır";
            //KomutVermeDeposu["Görsellerin Güncel Değerleri/Sabit 1", 0] = "Güncel Yazı";
            //KomutVermeDeposu["Görsellerin Güncel Değerleri/Sabit 2", 0] = @"C:\Deneme\Barkod.png";
            //BaşlangıçParamaetreleri = new string[] { ArgeMup.HazirKod.Dönüştürme.D_Yazı.Taban64e(KomutVermeDeposu.YazıyaDönüştür()) };

            ////Seçenek 2 C
            //KomutVermeDeposu["Komut"].İçeriği = new string[] { "Dosyaya Kaydet", @"C:\Deneme\Çıktı\Çıktı.png" };
            //KomutVermeDeposu["Görsellerin Güncel Değerleri/Sabit 1", 0] = "Güncel Yazı";
            //KomutVermeDeposu["Görsellerin Güncel Değerleri/Sabit 2", 0] = @"C:\Deneme\Barkod.png";
            //BaşlangıçParamaetreleri = new string[] { ArgeMup.HazirKod.Dönüştürme.D_Yazı.Taban64e(KomutVermeDeposu.YazıyaDönüştür()) };

            ////Seçenek 3
            //BaşlangıçParamaetreleri = new string[] { "YeniYazilimKontrolu" };

            ////Seçenek 4
            #endif

            if (BaşlangıçParamaetreleri != null && BaşlangıçParamaetreleri.Length == 1)
            {
                if (BaşlangıçParamaetreleri[0] == "YeniYazilimKontrolu")
                {
                    YeniYazılımKontrolü_ YeniYazılımKontrolü = new YeniYazılımKontrolü_();
                    YeniYazılımKontrolü.Başlat(new System.Uri("https://github.com/ArgeMup/Etiket/blob/main/Etiket/bin/Release/Etiket.exe?raw=true"));
                    while (!YeniYazılımKontrolü.KontrolTamamlandı) System.Threading.Thread.Sleep(1000);
                    YeniYazılımKontrolü.Durdur();
                    return;
                }
                else if (File.Exists(BaşlangıçParamaetreleri[0]))
                {
                    Ortak.Depo_Komut = new Depo_(File.ReadAllText(BaşlangıçParamaetreleri[0]));
                }
                else
                {
                    BaşlangıçParamaetreleri[0] = ArgeMup.HazirKod.Dönüştürme.D_Yazı.Taban64ten(BaşlangıçParamaetreleri[0]);
                    Ortak.Depo_Komut = new Depo_(BaşlangıçParamaetreleri[0]);
                }
            }

            if (Ortak.Depo_Komut == null)
            {
                Ortak.Depo_Komut = new Depo_();
                Ortak.Depo_Komut["Komut", 0] = "Ayarla";
                Ortak.Depo_Komut["Ayarlar", 0] = Kendi.Klasörü + "\\Ayarlar.mup";
            }

            Ortak.Depo_Ayarlar = new Depo_(File.Exists(Ortak.Depo_Komut["Ayarlar", 0]) ? File.ReadAllText(Ortak.Depo_Komut["Ayarlar", 0]) : null);
            
            if (Ortak.Depo_Komut["Komut", 0] == "Yazdır")
            {
                Ortak.Görseller_DizisiniOluştur(false, true, true);

                string snç = Ortak.Görseller_Görseli_Yazdır();
                if (!System.String.IsNullOrEmpty(snç)) File.WriteAllText(Kendi.Klasörü + "\\Hatalar.txt", snç);
                else if (File.Exists(Kendi.Klasörü + "\\Hatalar.txt")) File.Delete(Kendi.Klasörü + "\\Hatalar.txt");
                return;
            }
            else if (Ortak.Depo_Komut["Komut", 0] == "Dosyaya Kaydet")
            {
                Ortak.ArkaPlanRengi = Ortak.Renge(Ortak.Depo_Ayarlar.Oku_BaytDizisi("Kağıt"), System.Drawing.Color.Transparent);
                Ortak.KullanılabilirAlan_mm = new System.Drawing.SizeF((float)Ortak.Depo_Ayarlar.Oku_Sayı("Kağıt", 50, 1), (float)Ortak.Depo_Ayarlar.Oku_Sayı("Kağıt", 30, 2));
                Ortak.KullanılabilirAlan_piksel_Resim = new System.Drawing.Size((int)(Ortak.KullanılabilirAlan_mm.Width * Ortak.YakınlaşmaOranı / 0.254), (int)(Ortak.KullanılabilirAlan_mm.Height * Ortak.YakınlaşmaOranı / 0.254));

                Ortak.Görseller_DizisiniOluştur(false, true, false);

                string snç = Ortak.Görseller_Görseli_ResimHalineGetir(out System.Drawing.Image Resim);
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Ortak.Depo_Komut["Komut", 1]));
                    Resim.Save(Ortak.Depo_Komut["Komut", 1], System.Drawing.Imaging.ImageFormat.Png);
                }
                catch (System.Exception ex)
                {
                    snç += ex.Message;
                }

                if (!System.String.IsNullOrEmpty(snç)) File.WriteAllText(Kendi.Klasörü + "\\Hatalar.txt", snç);
                else if (File.Exists(Kendi.Klasörü + "\\Hatalar.txt")) File.Delete(Kendi.Klasörü + "\\Hatalar.txt");
                return;
            }
            else
            {
                Ortak.Görseller_DizisiniOluştur(true, true, false);
            }

            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new AnaEkran());
        }
    }
}
