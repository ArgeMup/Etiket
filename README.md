# Etiket
Genel amaçlı etiket üretme ve yazdırma uygulaması ArgeMup@yandex.com

    Tüm Ölçüler mm

    Komut satırından kullanım
        Seçenek 1 - Etiket.exe <Komut Verme Depo Dosyasının Yolu>
        Seçenek 2 - Etiket.exe <Komut Verme Depo Dosyası İçeriğinin Base64 Hali>
        Seçenek 3 - Etiket.exe YeniYazilimKontrolu                                      (Kontrolü bitirip kapanır)
        Seçenek 4 - Etiket.exe                                                          (Bu durumda değişiklikleri kendi alt klasörüne kaydeder)

    Komut Verme Depo Dosyası İçeriği
        Komut / Ayarla VEYA
        Komut / Yazdır VEYA / Kopya Sayısı
        Komut / Dosyaya Kaydet / <Kaydedilecek Klasörün Yolu>                           (Dosya adı -> <Şablon Adı>.png)
        Ayarlar / <Ayarlar Depo Dosya Yolu>
        Değişkenler                                                                     (Çağıran uygulamanın eklediği değişkenler - kullanıcı silemez)
            <Değişken Adı> / <İçeriği>

    Ayarlar Depo Dosyası İçeriği
        Şablonlar
            <Şablon Adı> / bit <Etkin>
                Kağıt / <Rengi> / Genişlik / Yükseklik
                Yazıcı / <Yazıcı adı> / Sayı <Soldan Boşluk> / Sayı <Üstten Boşluk>
                Görseller
                    <Görsel Adı> / bit Yazı_1_Resim_0 / bit Görünsün / Açı / Renk / Renk Arkaplan / Et Kalınlığı (Yazı için punto - 0 ise sığdır, Resim için çerçeve kalınlığı - 0 ise çizdirme)
                        Sol Üst Köşe / Sol / Üst / Genişlik / Yükseklik
                        İçerik / Yazıİçeriği_Veya_ResimDosyaYolu
                        Yazı / KarakterKümesi / Kalın / Yaslama_Yatay / Yaslama_Dikey
                        Resim / Çember çapı
        Değişkenler                                                                     (Kullanıcının eklediği değişkenler)
            <Değişken Adı> / <İçeriği>