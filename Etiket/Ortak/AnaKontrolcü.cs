using ArgeMup.HazirKod;
using ArgeMup.HazirKod.Ekİşlemler;
using System.IO;
using System.Windows.Forms;

namespace Etiket
{
    public class AnaKontrolcü
    {
        public static bool YanUygulamaOlarakÇalışıyor
        {
            get => Şube != null;
        }
        public static Form BoştaBekleyenAnaUygulama;
        static AnaEkran YanUygulamaAyarlamaPenceresi;
        static string YanUygulama_SonAçılan_Depo_Ayarlar_DosyaYolu;

        static YanUygulama.Şube_ Şube;
        static System.Threading.Mutex Kilit;

        public static void Açıl(int ŞubeErişimNoktası)
        {
            if (ŞubeErişimNoktası > 0)
            {
                Şube = new YanUygulama.Şube_(ŞubeErişimNoktası, GeriBildirim_İşlemi_Uygulama);
                Kilit = new System.Threading.Mutex();
                BoştaBekleyenAnaUygulama = new Form() { Opacity = 0, ShowInTaskbar = false, Visible = false };

#if DEBUG
                string komut = null;
                string ayarlar = null;
                Dosya.Yaz("Ayarlar.mup", ayarlar);

                //------------------------------------------

                byte[] dizi = komut.BaytDizisine();
                GeriBildirim_İşlemi_Uygulama(true, dizi, null);
                Application.Exit();
#endif
            }
            else
            {
                Ortak.Depo_Komut = new Depo_();
                Ortak.Depo_Komut["Komut", 0] = "Ayarla";
                Ortak.Depo_Komut["Ayarlar", 0] = Kendi.Klasörü + "\\Ayarlar.mup";

                Ortak.Depo_Ayarlar = new Depo_(Dosya.Oku_Yazı(Ortak.Depo_Komut["Ayarlar", 0]));
                Değişkenler.Başlat();

                BoştaBekleyenAnaUygulama = new AnaEkran();
            }
        }
        static void GeriBildirim_İşlemi_Uygulama(bool BağlantıKuruldu, byte[] Bilgi, string Açıklama)
        {
            if (!BağlantıKuruldu)
            {
#if !DEBUG
                Application.Exit();
#endif
                return;
            }

            if (YanUygulamaAyarlamaPenceresi != null)
            {
                try
                {
                    YanUygulamaAyarlamaPenceresi.Kaydet.Enabled = false;
                    YanUygulamaAyarlamaPenceresi.Dispose();
                }
                finally { YanUygulamaAyarlamaPenceresi = null; }
            }
            
            string içerik = Bilgi.Yazıya();
            if (içerik.BoşMu() || !Kilit.WaitOne(5000)) return;

            string snç_genel = null;

            try
            {
                Ortak.Depo_Komut = new Depo_(içerik);

                if (YanUygulama_SonAçılan_Depo_Ayarlar_DosyaYolu != Ortak.Depo_Komut["Ayarlar", 0])
                {
                    Ortak.Depo_Ayarlar = new Depo_(Dosya.Oku_Yazı(Ortak.Depo_Komut["Ayarlar", 0]));

                    YanUygulama_SonAçılan_Depo_Ayarlar_DosyaYolu = Ortak.Depo_Komut["Ayarlar", 0];
                }

                Değişkenler.Başlat();

                IDepo_Eleman Şablonlar = Ortak.Depo_Ayarlar["Şablonlar"];

                if (Ortak.Depo_Komut["Komut", 0] == "Yazdır")
                {
                    bool EnAz1EtkinŞablonVar = false;

                    for (int i = 0; i < Şablonlar.Elemanları.Length; i++)
                    {
                        if (!Şablonlar.Elemanları[i].Oku_Bit(null)) continue; //Etkin?
                        EnAz1EtkinŞablonVar = true;

                        Ortak.Görseller_DizisiniOluştur(Şablonlar.Elemanları[i], true);
                        Ortak.Görseller_YenidenHesaplat(Ortak.Renge(Ortak.Depo_Şablon.Oku_BaytDizisi("Kağıt"), System.Drawing.Color.Transparent));
                        Ortak.Görseller_YenidenHesaplat(1, Ortak.Depo_Şablon.Oku_Sayı("Kağıt", 50, 1), Ortak.Depo_Şablon.Oku_Sayı("Kağıt", 30, 2));

                        string snç_şablon = Ortak.Görseller_Görseli_Yazdır();
                        if (!string.IsNullOrEmpty(snç_şablon)) snç_genel += Şablonlar.Elemanları[i].Adı + " -> " + snç_şablon + System.Environment.NewLine + System.Environment.NewLine;
                    }

                    Ortak.Görseller_DizisiniYoket();

                    if (!EnAz1EtkinŞablonVar)
                    {
                        snç_genel = "Hiç etkin şablon bulunamadı.";
                    }

                    if (string.IsNullOrEmpty(snç_genel)) snç_genel = Ortak.Depo_Komut["Benzersiz_Tanımlayıcı", 0]; //Başarılı
                }
                else if (Ortak.Depo_Komut["Komut", 0] == "Dosyaya Kaydet")
                {
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
                            Resim.Dispose();
                        }
                        catch (System.Exception ex)
                        {
                            snç_genel += Şablonlar.Elemanları[i].Adı + " -> " + ex.Message + System.Environment.NewLine + System.Environment.NewLine;
                        }
                    }

                    Ortak.Görseller_DizisiniYoket();

                    if (!EnAz1EtkinŞablonVar)
                    {
                        snç_genel = "Hiç etkin şablon bulunamadı.";
                    }

                    if (string.IsNullOrEmpty(snç_genel)) snç_genel = Ortak.Depo_Komut["Benzersiz_Tanımlayıcı", 0]; //Başarılı
                }
                else if (Ortak.Depo_Komut["Komut", 0] == "Ayarla")
                {
                    BoştaBekleyenAnaUygulama.Invoke(new System.Action(() =>
                    {
                        YanUygulamaAyarlamaPenceresi = new AnaEkran();
                        YanUygulamaAyarlamaPenceresi.Show();

                        W32_3.SetForegroundWindow(YanUygulamaAyarlamaPenceresi.Handle);
                    }));

                    snç_genel = Ortak.Depo_Komut["Benzersiz_Tanımlayıcı", 0]; //Başarılı
                }
                else snç_genel = "Geçersiz komut " + Ortak.Depo_Komut["Komut", 0];
            }
            catch (System.Exception ex)
            {
                snç_genel += ex.Message;
            }

            Kilit.ReleaseMutex();

#if DEBUG
            snç_genel.Günlük();
#else
            byte[] cevap_dizi = snç_genel.BaytDizisine();
            if (cevap_dizi != null && cevap_dizi.Length > 0) Şube.Gönder(cevap_dizi);
#endif
        }

        public static void Kapan(string Bilgi)
        {
            Günlük.Ekle("Kapatıldı " + Bilgi, Hemen: true);

            Ortak.YeniYazılımKontrolü?.Durdur(); Ortak.YeniYazılımKontrolü = null;
            Şube?.Dispose(); Şube = null;
            Kilit?.Dispose(); Kilit = null;
            Ortak.Görseller_DizisiniYoket();

            ArgeMup.HazirKod.ArkaPlan.Ortak.Çalışsın = false;
        }
    }
}
