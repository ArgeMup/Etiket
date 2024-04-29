using ArgeMup.HazirKod;
using ArgeMup.HazirKod.Ekİşlemler;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Etiket
{
    public static class Ortak
    {
        public static YeniYazılımKontrolü_ YeniYazılımKontrolü;
        public static Depo_ Depo_Komut = null, Depo_Ayarlar = null;
        public static IDepo_Eleman Depo_Şablon = null;
        public static Görsel[] Görsel_ler = null;
        public static SizeF KullanılabilirAlan_mm;
        public static Size KullanılabilirAlan_piksel_Resim;
        public static Color ArkaPlanRengi = Color.Transparent;
        public static Görsel SeçiliGörsel = null;
        public static float YakınlaşmaOranı = 1;
        static SolidBrush Fırça_ArkaPlanRengi = null;

        public static Color Renge(byte[] Girdi, Color BulunamamasıDurumundakiDeğeri)
        {
            if (Girdi == null || Girdi.Length != 4) return BulunamamasıDurumundakiDeğeri;
            return Color.FromArgb(Girdi[0], Girdi[1], Girdi[2], Girdi[3]);
        }
        public static string Renkten(Color Renk)
        {
            byte[] Girdi = new byte[4];
            Girdi[0] = Renk.A;
            Girdi[1] = Renk.R;
            Girdi[2] = Renk.G;
            Girdi[3] = Renk.B;

            return ArgeMup.HazirKod.Dönüştürme.D_HexYazı.BaytDizisinden(Girdi);
        }

        public class Görsel : IDisposable
        {
            #region Değişkenler
            //Genel
            public string Adı;
            public RectangleF Çerçeve;
            public bool Yazı_1_Resim_0, Görünsün;
            public float Açı, EtKalınlığı;
            public Color Renk, Renk_ArkaPlan;

            public string Yazıİçeriği_Veya_ResimDosyaYolu;
            public string Yazıİçeriği_Veya_ResimDosyaYolu_Çözümlenmiş
            {
                get
                {
                    return Değişkenler.Çözümle(Yazıİçeriği_Veya_ResimDosyaYolu);
                }
            }

            //Genel Yazı
            public string Yazı_KarakterKümesi;
            public int Yazı_Yaslama_Yatay, Yazı_Yaslama_Dikey;
            public bool Yazı_Kalın;
            //Genel Resim
            public float Resim_Yuvarlama_Çap;

            //İç Kullanım Yazı
            public Font Yazı_KarakterKümesi_;
            public SolidBrush Fırça_, Fırça_ArkaPlan_;
            public StringFormat Yazı_Şekli_;
            //İç Kullanım Resim
            public Image Resim_;
            public Pen Resim_Kalem_;
            public System.Drawing.Drawing2D.GraphicsPath Resim_Çerçeve_Yolu_;
            #endregion

            public Görsel(IDepo_Eleman ŞablonDalı)
            {
                if (ŞablonDalı == null) return;

                Adı = ŞablonDalı.Adı;
                Yazı_1_Resim_0 = ŞablonDalı.Oku_Bit(null, true, 0);
                Görünsün = ŞablonDalı.Oku_Bit(null, true, 1);
                Açı = (float)ŞablonDalı.Oku_Sayı(null, 0, 2);
                Renk = Renge(ŞablonDalı.Oku_BaytDizisi(null, null, 3), Color.Black);
                Renk_ArkaPlan = Renge(ŞablonDalı.Oku_BaytDizisi(null, null, 4), Color.Yellow);
                EtKalınlığı = (float)ŞablonDalı.Oku_Sayı(null, Yazı_1_Resim_0 ? 0 : 1, 5);

                IDepo_Eleman k = ŞablonDalı["Sol Üst Köşe"];
                Çerçeve = new RectangleF((float)k.Oku_Sayı(null, 0, 0), (float)k.Oku_Sayı(null, 0, 1), (float)k.Oku_Sayı(null, 10, 2), (float)k.Oku_Sayı(null, 5, 3));

                Yazıİçeriği_Veya_ResimDosyaYolu = ŞablonDalı.Oku("İçerik");

                k = ŞablonDalı["Yazı"];
                Yazı_KarakterKümesi = k.Oku(null, "Calibri", 0);
                Yazı_Kalın = k.Oku_Bit(null, false, 1);
                Yazı_Yaslama_Yatay = k.Oku_TamSayı(null, 1, 2);
                Yazı_Yaslama_Dikey = k.Oku_TamSayı(null, 1, 3);

                k = ŞablonDalı["Resim"];
                Resim_Yuvarlama_Çap = (float)k.Oku_Sayı(null, 2, 0);
            }
            public void Çizdir(Graphics Grafik, bool Seçildi)
            {
                if (Açı != 0)
                {
                    float _x = Çerçeve.X + (Çerçeve.Width / 2), _y = Çerçeve.Y + (Çerçeve.Height / 2);
                    Grafik.TranslateTransform(_x, _y); // döndürme noktası
                    Grafik.RotateTransform(Açı);
                    Grafik.TranslateTransform(-_x, -_y);
                }

                string Çözümlenmiş_İçerik = Yazıİçeriği_Veya_ResimDosyaYolu_Çözümlenmiş;

                if (Yazı_1_Resim_0)
                {
                    if (Yazı_KarakterKümesi_ == null)
                    {
                        Fırça_ = new SolidBrush(Renk);
                        Fırça_ArkaPlan_ = new SolidBrush(Renk_ArkaPlan);
                        Yazı_Şekli_ = new StringFormat
                        {
                            LineAlignment = (StringAlignment)Yazı_Yaslama_Dikey,
                            Alignment = (StringAlignment)Yazı_Yaslama_Yatay
                        };

                        float KaBü_ = EtKalınlığı;
                        if (KaBü_ == 0)
                        {
                            if (string.IsNullOrEmpty(Çözümlenmiş_İçerik)) Çözümlenmiş_İçerik = " ";

                            SizeF Ölçü = new SizeF();
                            int KarakterSayısı = Çözümlenmiş_İçerik.Length, YazdırılanAdet = KarakterSayısı;

                            while (Ölçü.Width < Çerçeve.Width && Ölçü.Height < Çerçeve.Height && KarakterSayısı == YazdırılanAdet)
                            {
                                KaBü_ += 5;
                                Yazı_KarakterKümesi_ = new Font(Yazı_KarakterKümesi, KaBü_, Yazı_Kalın ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Millimeter);
                                Ölçü = Grafik.MeasureString(Çözümlenmiş_İçerik, Yazı_KarakterKümesi_, Çerçeve.Size, Yazı_Şekli_, out YazdırılanAdet, out _);
                                Yazı_KarakterKümesi_.Dispose();
                            }

                            while (Ölçü.Width >= Çerçeve.Width || Ölçü.Height >= Çerçeve.Height || KarakterSayısı != YazdırılanAdet)
                            {
                                KaBü_ -= 0.1f;
                                Yazı_KarakterKümesi_ = new Font(Yazı_KarakterKümesi, KaBü_, Yazı_Kalın ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Millimeter);
                                Ölçü = Grafik.MeasureString(Çözümlenmiş_İçerik, Yazı_KarakterKümesi_, Çerçeve.Size, Yazı_Şekli_, out YazdırılanAdet, out _);
                                Yazı_KarakterKümesi_.Dispose();
                            }
                        }

                        Yazı_KarakterKümesi_ = new Font(Yazı_KarakterKümesi, KaBü_, Yazı_Kalın ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Millimeter);
                        if (Yazı_KarakterKümesi_.Name != Yazı_KarakterKümesi_.OriginalFontName) throw new System.Exception("Kayıtlı olan (" + Yazı_KarakterKümesi + ") mevcut karakter kümeleri arasında bulunmadığından açılamadı");
                    }

                    if (Renk_ArkaPlan != Color.Transparent) Grafik.FillRectangle(Fırça_ArkaPlan_, Çerçeve);

                    Grafik.DrawString(Çözümlenmiş_İçerik, Yazı_KarakterKümesi_, Fırça_, Çerçeve, Yazı_Şekli_);
                }
                else
                {
                    //Resimi hazırlama
                    if (Resim_ == null && Çözümlenmiş_İçerik.DoluMu(true))
                    {
                        Resim_ = Image.FromFile(Çözümlenmiş_İçerik);
                    }

                    if (Çerçeve.Width == 0)
                    {
                        if (Resim_ != null)
                        {
                            Grafik.PageUnit = GraphicsUnit.Pixel;
                            Grafik.DrawImageUnscaled(Resim_, (int)Çerçeve.Location.X, (int)Çerçeve.Location.Y);
                            Grafik.PageUnit = GraphicsUnit.Millimeter;
                        }
                    }
                    else
                    {
                        //Ön Hazırlık
                        if (EtKalınlığı > 0 || Renk_ArkaPlan != Color.Transparent)
                        {
                            if (Resim_Çerçeve_Yolu_ == null)
                            {
                                Resim_Çerçeve_Yolu_ = new System.Drawing.Drawing2D.GraphicsPath();
                                float EtKalınlığı_Bölü2 = EtKalınlığı / 2;
                                RectangleF çember = new RectangleF(Çerçeve.X + EtKalınlığı_Bölü2, Çerçeve.Y + EtKalınlığı_Bölü2, Çerçeve.Width - EtKalınlığı, Çerçeve.Height - EtKalınlığı);

                                if (Resim_Yuvarlama_Çap == 0)
                                {
                                    Resim_Çerçeve_Yolu_.AddRectangle(çember);
                                }
                                else
                                {
                                    RectangleF daire = new RectangleF(çember.Location, new SizeF(Resim_Yuvarlama_Çap, Resim_Yuvarlama_Çap));

                                    // sol üst
                                    Resim_Çerçeve_Yolu_.AddArc(daire, 180, 90);

                                    // sağ üst 
                                    daire.X = çember.Right - Resim_Yuvarlama_Çap;
                                    Resim_Çerçeve_Yolu_.AddArc(daire, 270, 90);

                                    // sağ alt
                                    daire.Y = çember.Bottom - Resim_Yuvarlama_Çap;
                                    Resim_Çerçeve_Yolu_.AddArc(daire, 0, 90);

                                    // sol alt
                                    daire.X = çember.Left;
                                    Resim_Çerçeve_Yolu_.AddArc(daire, 90, 90);
                                }

                                Resim_Çerçeve_Yolu_.CloseFigure(); 
                            }
                        }

                        //En alta resim arka plan boyama
                        if (Renk_ArkaPlan != Color.Transparent)
                        {
                            if (Fırça_ArkaPlan_ == null)
                            {
                                Fırça_ArkaPlan_ = new SolidBrush(Renk_ArkaPlan);
                            }

                            Grafik.FillPath(Fırça_ArkaPlan_, Resim_Çerçeve_Yolu_);
                        }

                        //Arkaplan üstüne resimi çizdirme
                        if (Resim_ != null)
                        {
                            Grafik.DrawImage(Resim_, Çerçeve);
                        }

                        //Resimin üzerine çerçeve çizdirme
                        if (EtKalınlığı > 0)
                        {
                            if (Fırça_ == null)
                            {
                                Fırça_ = new SolidBrush(Renk);
                                Resim_Kalem_ = new Pen(Fırça_, EtKalınlığı);
                            }

                            Grafik.DrawPath(Resim_Kalem_, Resim_Çerçeve_Yolu_);
                        }
                    }
                }

                if (Seçildi && Çerçeve.Width > 0)
                {
                    RectangleF r = Çerçeve;
                    Pen p_k = new Pen(Brushes.Red, 0.2f);
                    Pen p_b = new Pen(Brushes.White, 0.2f);

                    r.Inflate(0.1f, 0.1f);
                    Grafik.DrawRectangle(p_k, r.X, r.Y, r.Width, r.Height);

                    r.Inflate(0.2f, 0.2f);
                    Grafik.DrawRectangle(p_b, r.X, r.Y, r.Width, r.Height);

                    r.Inflate(0.2f, 0.2f);
                    Grafik.DrawRectangle(p_k, r.X, r.Y, r.Width, r.Height);

                    r.Inflate(0.2f, 0.2f);
                    Grafik.DrawRectangle(p_b, r.X, r.Y, r.Width, r.Height);

                    r.Inflate(0.2f, 0.2f);
                    Grafik.DrawRectangle(p_k, r.X, r.Y, r.Width, r.Height);

                    p_k.Dispose();
                    p_b.Dispose();
                }

                if (Açı != 0)
                {
                    Grafik.RotateTransform(-Açı);
                }

                Grafik.ResetTransform();
            }
            public void YenidenHesaplat()
            {
                Yazı_KarakterKümesi_?.Dispose(); Yazı_KarakterKümesi_ = null;
                Fırça_?.Dispose(); Fırça_ = null;
                Fırça_ArkaPlan_?.Dispose(); Fırça_ArkaPlan_ = null;
                Yazı_Şekli_?.Dispose(); Yazı_Şekli_ = null;

                Resim_?.Dispose(); Resim_ = null;
                Resim_Kalem_?.Dispose(); Resim_Kalem_ = null;
                Resim_Çerçeve_Yolu_?.Dispose(); Resim_Çerçeve_Yolu_ = null;
            }
            public void Depo_Kaydet(IDepo_Eleman ŞablonDalı)
            {
                IDepo_Eleman Detaylar = ŞablonDalı["Görseller/" + Adı];

                Detaylar.Adı = Adı;
                Detaylar.Yaz(null, Yazı_1_Resim_0, 0);
                Detaylar.Yaz(null, Görünsün, 1);
                Detaylar.Yaz(null, Açı, 2);
                Detaylar.Yaz(null, Renkten(Renk), 3);
                Detaylar.Yaz(null, Renkten(Renk_ArkaPlan), 4);
                Detaylar.Yaz(null, EtKalınlığı, 5);

                IDepo_Eleman a = Detaylar["Sol Üst Köşe"];
                a.Yaz(null, Çerçeve.X, 0);
                a.Yaz(null, Çerçeve.Y, 1);
                a.Yaz(null, Çerçeve.Width, 2);
                a.Yaz(null, Çerçeve.Height, 3);

                Detaylar.Yaz("İçerik", Yazıİçeriği_Veya_ResimDosyaYolu);

                Detaylar["Yazı"].İçeriği = new string[] { Yazı_KarakterKümesi, Yazı_Kalın.ToString(), Yazı_Yaslama_Yatay.ToString(), Yazı_Yaslama_Dikey.ToString() };

                Detaylar.Yaz("Resim", Resim_Yuvarlama_Çap);
            }

            #region Idisposable
            private bool disposedValue;
            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: dispose managed state (managed objects)

                        YenidenHesaplat();
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                    // TODO: set large fields to null
                    disposedValue = true;
                }
            }

            // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
            ~Görsel()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: false);
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
            #endregion
        }
        public static void Görseller_DizisiniOluştur(IDepo_Eleman ŞablonDalı, bool BoşluklarıEkle)
        {
            Görseller_DizisiniYoket();

            Depo_Şablon = ŞablonDalı;

            Görsel_ler = new Görsel[Depo_Şablon["Görseller"].Elemanları.Length];
            SizeF Boşluk = new SizeF((float)Depo_Şablon.Oku_Sayı("Yazıcı", 0, 1), (float)Depo_Şablon.Oku_Sayı("Yazıcı", 0, 2));
         
            for (int i = 0; i < Görsel_ler.Length; i++)
            {
                Görsel G = new Görsel(Depo_Şablon["Görseller"].Elemanları[i]);
                if (BoşluklarıEkle) G.Çerçeve.Location = PointF.Add(G.Çerçeve.Location, Boşluk); //yazıcı ölü alanının eklenmesi
                Görsel_ler[i] = G;
            }
        }
        public static void Görseller_DizisiniYoket()
        {
            if (Görsel_ler != null)
            {
                foreach (Görsel g in Görsel_ler)
                {
                    g.Dispose();
                }
            }

            Görsel_ler = null;
        }
        public static void Görseller_YenidenHesaplat(Color İstenen_ArkaPlanRengi)
        {
            ArkaPlanRengi = İstenen_ArkaPlanRengi;

            Fırça_ArkaPlanRengi?.Dispose(); Fırça_ArkaPlanRengi = null;
            if (İstenen_ArkaPlanRengi != Color.Transparent) Fırça_ArkaPlanRengi = new SolidBrush(İstenen_ArkaPlanRengi);
        }
        public static void Görseller_YenidenHesaplat(double İstenen_YakınlaşmaOranı, double İstenen_Genişlik, double İstenen_YÜkseklik)
        {
            YakınlaşmaOranı = (float)İstenen_YakınlaşmaOranı;

            KullanılabilirAlan_mm = new SizeF((float)İstenen_Genişlik, (float)İstenen_YÜkseklik);

            float Çarpan = 25.4f / 96.0f; //ekran dpi oranı https://learn.microsoft.com/en-us/windows/win32/learnwin32/dpi-and-device-independent-pixels
            KullanılabilirAlan_piksel_Resim = new Size((int)(KullanılabilirAlan_mm.Width * YakınlaşmaOranı / Çarpan), (int)(KullanılabilirAlan_mm.Height * YakınlaşmaOranı / Çarpan));
        }
        public static Görsel Görseller_Görseli_Bul(string Adı)
        {
            for (int i = 0; i < Görsel_ler.Length; i++)
            {
                if (Görsel_ler[i].Adı == Adı) return Görsel_ler[i];
            }

            return null;
        }
        public static string Görseller_Görseli_Üret(Graphics Grafik)
        {
            string Hatalar = null;
            Grafik.ResetTransform();
            Grafik.PageUnit = GraphicsUnit.Millimeter;

            if (ArkaPlanRengi != Color.Transparent)
            {
                if (YakınlaşmaOranı != 1) Grafik.ScaleTransform(YakınlaşmaOranı, YakınlaşmaOranı);
                Grafik.FillRectangle(Fırça_ArkaPlanRengi, 0, 0, KullanılabilirAlan_mm.Width, KullanılabilirAlan_mm.Height);
                Grafik.ResetTransform();
            }

            if (Görsel_ler != null)
            {
                for (int i = Görsel_ler.Length - 1; i >= 0; i--)
                {
                    if (!Görsel_ler[i].Görünsün || Görsel_ler[i] == SeçiliGörsel) continue;

                    try
                    {
                        if (YakınlaşmaOranı != 1) Grafik.ScaleTransform(YakınlaşmaOranı, YakınlaşmaOranı);
                        Görsel_ler[i].Çizdir(Grafik, false);
                    }
                    catch (System.Exception ex)
                    {
                        Grafik.ResetTransform();
                        Hatalar += Görsel_ler[i].Adı + " -> " + ex.Message;
                    }
                }

                //Seçilenin üstte görünmesi için en son çizdir
                if (SeçiliGörsel != null && SeçiliGörsel.Görünsün)
                {
                    try
                    {
                        if (YakınlaşmaOranı != 1) Grafik.ScaleTransform(YakınlaşmaOranı, YakınlaşmaOranı);
                        SeçiliGörsel.Çizdir(Grafik, true);
                    }
                    catch (System.Exception ex)
                    {
                        Grafik.ResetTransform();
                        Hatalar += SeçiliGörsel.Adı + " -> " + ex.Message;
                    }
                }
            }
            else Hatalar += "Görsel_ler nesnesi boş durumda iken çizildi.";

            return Hatalar;
        }
        public static string Görseller_Görseli_Yazdır()
        {
            string Hatalar = null;

            System.Drawing.Printing.PrintDocument pd = new System.Drawing.Printing.PrintDocument();
            pd.PrinterSettings.PrinterName = Depo_Şablon.Oku("Yazıcı", null, 0);
            pd.PrinterSettings.Copies = (short)Depo_Komut.Oku_TamSayı("Komut", 1, 1);
            pd.PrintController = new System.Drawing.Printing.StandardPrintController(); //Yazdırılıyor yazısının gizlenmesi
            pd.OriginAtMargins = false;
            pd.PrintPage += pd_Yazdır;
            if (pd.PrinterSettings.IsValid) pd.Print();
            else Hatalar += "Yazıcıya (" + pd.PrinterSettings.PrinterName + ") ulaşılamadı.";
            pd.Dispose();
            return Hatalar;

            void pd_Yazdır(object senderr, System.Drawing.Printing.PrintPageEventArgs ev)
            {
                Hatalar += Görseller_Görseli_Üret(ev.Graphics);
            }
        }
        public static string Görseller_Görseli_ResimHalineGetir(out Image Resim)
        {
            Resim = new Bitmap(KullanılabilirAlan_piksel_Resim.Width, KullanılabilirAlan_piksel_Resim.Height);
            Graphics Grafik = Graphics.FromImage(Resim);
            string Hatalar = Görseller_Görseli_Üret(Grafik);
            Grafik.Dispose();
            return Hatalar;
        }
    }

    public static class Değişkenler
    {
        //Çözümleme için girilecek değiken adı  -> %Değişken Adı%
        //liste içerisinde tutulan değişken adı -> Değişken Adı 

        public static Dictionary<string, string> Liste_UygulamaEkledi = new Dictionary<string, string>();
        public static Dictionary<string, string> Liste_KullanıcıEkledi = new Dictionary<string, string>();

        public static void Başlat()
        {
            Liste_UygulamaEkledi.Clear();
            Liste_KullanıcıEkledi.Clear();

            if (Ortak.Depo_Komut != null)
            {
                IDepo_Eleman DeğişkenlerDalı = Ortak.Depo_Komut["Değişkenler"];
                foreach (IDepo_Eleman Değişken in DeğişkenlerDalı.Elemanları)
                {
                    if (Değişken.İçiBoşOlduğuİçinSilinecek) continue;

                    Liste_UygulamaEkledi.Add(Değişken.Adı, Değişken[0]);
                }
            }

            if (Ortak.Depo_Ayarlar != null)
            {
                IDepo_Eleman DeğişkenlerDalı = Ortak.Depo_Ayarlar["Değişkenler"];
                foreach (IDepo_Eleman Değişken in DeğişkenlerDalı.Elemanları)
                {
                    if (Değişken.İçiBoşOlduğuİçinSilinecek) continue;

                    Liste_KullanıcıEkledi.Add(Değişken.Adı, Değişken[0]);
                    
                }
            }
        }
        public static void Kaydet()
        {
            IDepo_Eleman DeğişkenlerDalı = Ortak.Depo_Ayarlar["Değişkenler"];
            DeğişkenlerDalı.Sil(null, true, true);

            foreach (var biri in Liste_KullanıcıEkledi)
            {
                DeğişkenlerDalı.Yaz(biri.Key, biri.Value);

                if (biri.Value.BoşMu(true)) DeğişkenlerDalı.Yaz(biri.Key, " ", 1); //boş iiçerikli değişkenin kaybolmaması için
            }
        }
        public static void EkleVeyaDeğiştir(string Adı, string İçeriği)
        {
            if (Adı.BoşMu(true)) return;

            if (Liste_KullanıcıEkledi.ContainsKey(Adı)) Liste_KullanıcıEkledi[Adı] = İçeriği;
            else Liste_KullanıcıEkledi.Add(Adı, İçeriği);
        }
        public static string Çözümle(string Girdi)
        {
            if (Girdi.BoşMu(true)) return Girdi;

            foreach (var biri in Liste_UygulamaEkledi)
            {
                Girdi = Girdi.Replace("%" + biri.Key + "%", biri.Value);
            }

            foreach (var biri in Liste_KullanıcıEkledi)
            {
                Girdi = Girdi.Replace("%" + biri.Key + "%", biri.Value);
            }

            return Girdi;
        }
    }
}
