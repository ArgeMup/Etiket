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
            //KomutVermeDeposu["Değişkenler/Değişken 1", 0] = "De 1 içeriği";
            //KomutVermeDeposu["Değişkenler/Değişken 2", 0] = "De 2 içeriği";
            //BaşlangıçParamaetreleri = new string[] { @"C:\Deneme\Komut.txt" };
            //File.WriteAllText(BaşlangıçParamaetreleri[0], KomutVermeDeposu.YazıyaDönüştür());

            ////Seçenek 2 A
            //KomutVermeDeposu["Komut", 0] = "Ayarla";
            //KomutVermeDeposu["Değişkenler/Değişken 1", 0] = "De 1 içeriği";
            //KomutVermeDeposu["Değişkenler/Değişken 2", 0] = "De 2 içeriği";
            //BaşlangıçParamaetreleri = new string[] { ArgeMup.HazirKod.Dönüştürme.D_Yazı.Taban64e(KomutVermeDeposu.YazıyaDönüştür()) };

            ////Seçenek 2 B
            //KomutVermeDeposu["Komut", 0] = "Yazdır";
            //KomutVermeDeposu["Değişkenler/Değişken 1", 0] = "De 1 canlı çalışma içeriği";
            //KomutVermeDeposu["Değişkenler/Değişken 2", 0] = "De 2 canlı çalışma içeriği";
            //BaşlangıçParamaetreleri = new string[] { ArgeMup.HazirKod.Dönüştürme.D_Yazı.Taban64e(KomutVermeDeposu.YazıyaDönüştür()) };

            ////Seçenek 2 C
            //KomutVermeDeposu["Komut"].İçeriği = new string[] { "Dosyaya Kaydet", @"C:\Deneme\Çıktı" };
            //KomutVermeDeposu["Değişkenler/Değişken 1", 0] = "De 1 canlı çalışma içeriği";
            //KomutVermeDeposu["Değişkenler/Değişken 2", 0] = "De 2 canlı çalışma içeriği";
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
            IDepo_Eleman Şablonlar = Ortak.Depo_Ayarlar["Şablonlar"];
            Değişkenler.Başlat();

            if (Ortak.Depo_Komut["Komut", 0] == "Yazdır")
            {
                bool EnAz1EtkinŞablonVar = false;
                string snç_genel = null;

                for (int i = 0; i < Şablonlar.Elemanları.Length; i++)
                {
                    if (!Şablonlar.Elemanları[i].Oku_Bit(null)) continue; //Etkin?
                    EnAz1EtkinŞablonVar = true;

                    Ortak.Görseller_DizisiniOluştur(Şablonlar.Elemanları[i], true);

                    string snç_şablon = Ortak.Görseller_Görseli_Yazdır();
                    if (!string.IsNullOrEmpty(snç_şablon)) snç_genel += Şablonlar.Elemanları[i].Adı + " -> " + snç_şablon + System.Environment.NewLine + System.Environment.NewLine;
                }

                if (!EnAz1EtkinŞablonVar)
                {
                    snç_genel = "Hiç etkin şablon bulunamadı.";
                }

                if (!string.IsNullOrEmpty(snç_genel)) File.WriteAllText(Kendi.Klasörü + "\\Hatalar.txt", snç_genel);
                else if (File.Exists(Kendi.Klasörü + "\\Hatalar.txt")) File.Delete(Kendi.Klasörü + "\\Hatalar.txt");
                return;
            }
            else if (Ortak.Depo_Komut["Komut", 0] == "Dosyaya Kaydet")
            {
                string snç_genel = null;
                int adet = Şablonlar.Elemanları.Length;
                bool EnAz1EtkinŞablonVar = false;

                for (int i = 0; i < adet; i++)
                {
                    if (!Şablonlar.Elemanları[i].Oku_Bit(null)) continue; //Etkin?
					EnAz1EtkinŞablonVar = true;

                    Ortak.Görseller_DizisiniOluştur(Şablonlar.Elemanları[i], false);
                    Ortak.Görseller_YenidenHesaplat(Ortak.Renge(Ortak.Depo_Şablon.Oku_BaytDizisi("Kağıt"), System.Drawing.Color.Transparent));
                    Ortak.Görseller_YenidenHesaplat(1, Ortak.Depo_Şablon.Oku_Sayı("Kağıt", 50, 1), Ortak.Depo_Şablon.Oku_Sayı("Kağıt", 30, 2));

                    string snç_şablon = Ortak.Görseller_Görseli_ResimHalineGetir(out System.Drawing.Image Resim);
                    if (!string.IsNullOrEmpty(snç_şablon)) snç_genel += Şablonlar.Elemanları[i].Adı + " -> " + snç_şablon + System.Environment.NewLine + System.Environment.NewLine;

                    try
                    {
                        Klasör.Oluştur(Ortak.Depo_Komut["Komut", 1]);
                        Resim.Save(Ortak.Depo_Komut["Komut", 1] + "\\" + Şablonlar.Elemanları[i].Adı + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    }
                    catch (System.Exception ex)
                    {
                        snç_genel += Şablonlar.Elemanları[i].Adı + " -> " + ex.Message + System.Environment.NewLine + System.Environment.NewLine;
                    }
                }

                if (!EnAz1EtkinŞablonVar)
                {
                    snç_genel = "Hiç etkin şablon bulunamadı.";
                }

                if (!System.String.IsNullOrEmpty(snç_genel)) File.WriteAllText(Kendi.Klasörü + "\\Hatalar.txt", snç_genel);
                else if (File.Exists(Kendi.Klasörü + "\\Hatalar.txt")) File.Delete(Kendi.Klasörü + "\\Hatalar.txt");
                return;
            }

            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new AnaEkran());
        }
    }
}
