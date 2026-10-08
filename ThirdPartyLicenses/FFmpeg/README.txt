FFmpeg 64-bit static Windows build from www.gyan.dev

Version: 2026-09-02-git-9fc8c785e2-full_build-www.gyan.dev

License: GPL v3

Source Code: https://github.com/FFmpeg/FFmpeg/commit/9fc8c785e2

External Assets
frei0r plugins:   https://www.gyan.dev/ffmpeg/builds/ffmpeg-frei0r-plugins
lensfun database: https://www.gyan.dev/ffmpeg/builds/ffmpeg-lensfun-db
whisper models:   https://huggingface.co/ggerganov/whisper.cpp/tree/main

git-full build configuration: 

ARCH                      x86 (generic)
big-endian                no
runtime cpu detection     yes
standalone assembly       yes
x86 assembler             nasm
MMX enabled               yes
MMXEXT enabled            yes
SSE enabled               yes
SSSE3 enabled             yes
AESNI enabled             yes
CLMUL enabled             yes
AVX enabled               yes
AVX2 enabled              yes
AVX-512 enabled           yes
AVX-512ICL enabled        yes
XOP enabled               yes
FMA3 enabled              yes
FMA4 enabled              yes
i686 features enabled     yes
CMOV is fast              yes
EBX available             yes
6 registers available     yes
7 registers available     yes
debug symbols             yes
strip symbols             yes
optimize for size         no
optimizations             yes
static                    yes
shared                    no
network support           yes
threading support         pthreads
safe bitstream reader     yes
texi2html enabled         no
perl enabled              yes
pod2man enabled           yes
makeinfo enabled          yes
makeinfo supports HTML    yes
experimental features     yes
xmllint enabled           yes

External libraries:
avisynth                libgsm                  libsvtjpegxs
bzlib                   libharfbuzz             libtheora
cairo                   libilbc                 libtwolame
chromaprint             libjxl                  libuavs3d
frei0r                  liblc3                  libvidstab
gmp                     liblensfun              libvmaf
gnutls                  libmodplug              libvo_amrwbenc
iconv                   libmp3lame              libvorbis
ladspa                  libmysofa               libvpx
lcms2                   liboapv                 libvvenc
libaom                  libopencore_amrnb       libwebp
libaribb24              libopencore_amrwb       libx264
libaribcaption          libopenjpeg             libx265
libass                  libopenmpt              libxavs2
libbluray               libopus                 libxevd
libbs2b                 libplacebo              libxeve
libcaca                 libqrencode             libxml2
libcdio                 libquirc                libxvid
libcodec2               librav1e                libzimg
libdav1d                librist                 libzmq
libdavs2                librubberband           libzvbi
libdvdnav               libshine                lzma
libdvdread              libsnappy               mediafoundation
libflite                libsoxr                 openal
libfontconfig           libspeex                sdl2
libfreetype             libsrt                  whisper
libfribidi              libssh                  zlib
libgme                  libsvtav1

External libraries providing hardware acceleration:
amf                     d3d12va                 nvdec
cuda                    dxva2                   nvenc
cuda_llvm               ffnvcodec               opencl
cuvid                   libmfx                  vaapi
d3d11va                 libvpl                  vulkan

Libraries:
avcodec                 avformat                swscale
avdevice                avutil
avfilter                swresample

Programs:
ffmpeg                  ffplay                  ffprobe

Enabled decoders:
aac                     flv                     pcm_u16le
aac_fixed               fmvc                    pcm_u24be
aac_latm                fourxm                  pcm_u24le
aasc                    fraps                   pcm_u32be
ac3                     frwu                    pcm_u32le
ac3_fixed               ftr                     pcm_u8
acelp_kelvin            g2m                     pcm_vidc
adpcm_4xm               g723_1                  pcx
adpcm_adx               g728                    pdv
adpcm_afc               g729                    pfm
adpcm_agm               gdv                     pgm
adpcm_aica              gem                     pgmyuv
adpcm_argo              gif                     pgssub
adpcm_circus            gremlin_dpcm            pgx
adpcm_ct                gsm                     phm
adpcm_dtk               gsm_ms                  photocd
adpcm_ea                h261                    pictor
adpcm_ea_maxis_xa       h263                    pixlet
adpcm_ea_r1             h263i                   pjs
adpcm_ea_r2             h263p                   png
adpcm_ea_r3             h264                    ppm
adpcm_ea_xas            h264_amf                prores
adpcm_g722              h264_cuvid              prores_raw
adpcm_g726              h264_qsv                prosumer
adpcm_g726le            hap                     psd
adpcm_ima_acorn         hca                     ptx
adpcm_ima_alp           hcom                    qcelp
adpcm_ima_amv           hdr                     qdm2
adpcm_ima_apc           hevc                    qdmc
adpcm_ima_apm           hevc_amf                qdraw
adpcm_ima_cunning       hevc_cuvid              qoa
adpcm_ima_dat4          hevc_qsv                qoi
adpcm_ima_dk3           hnm4_video              qpeg
adpcm_ima_dk4           hq_hqa                  qtrle
adpcm_ima_ea_eacs       hqx                     r10k
adpcm_ima_ea_sead       huffyuv                 r210
adpcm_ima_escape        hymt                    ra_144
adpcm_ima_hvqm2         iac                     ra_288
adpcm_ima_hvqm4         idcin                   ralf
adpcm_ima_iss           idf                     rasc
adpcm_ima_magix         iff_ilbm                rawvideo
adpcm_ima_moflex        ilbc                    realtext
adpcm_ima_mtf           imc                     rka
adpcm_ima_oki           imm4                    rl2
adpcm_ima_pda           imm5                    roq
adpcm_ima_qt            indeo2                  roq_dpcm
adpcm_ima_rad           indeo3                  rpza
adpcm_ima_smjpeg        indeo4                  rscc
adpcm_ima_ssi           indeo5                  rtv1
adpcm_ima_wav           interplay_acm           rv10
adpcm_ima_ws            interplay_dpcm          rv20
adpcm_ima_xbox          interplay_video         rv30
adpcm_ms                ipu                     rv40
adpcm_mtaf              jacosub                 rv60
adpcm_n64               jpeg2000                s302m
adpcm_psx               jpegls                  sami
adpcm_psxc              jv                      sanm
adpcm_sanyo             kgv1                    sbc
adpcm_sbpro_2           kmvc                    scpr
adpcm_sbpro_3           lagarith                screenpresso
adpcm_sbpro_4           lead                    sdx2_dpcm
adpcm_swf               libaom_av1              sga
adpcm_thp               libaribb24              sgi
adpcm_thp_le            libaribcaption          sgirle
adpcm_vima              libcodec2               sheervideo
adpcm_xa                libdav1d                shorten
adpcm_xmd               libdavs2                simbiosis_imx
adpcm_yamaha            libgsm                  sipr
adpcm_zork              libgsm_ms               siren
agm                     libilbc                 smackaud
ahx                     libjxl                  smacker
aic                     libjxl_anim             smc
alac                    liblc3                  smvjpeg
alias_pix               libopencore_amrnb       snow
als                     libopencore_amrwb       sol_dpcm
amrnb                   libopus                 sp5x
amrwb                   libspeex                speedhq
amv                     libsvtjpegxs            speex
anm                     libuavs3d               srgc
ansi                    libvorbis               srt
anull                   libvpx_vp8              ssa
apac                    libvpx_vp9              stl
ape                     libxevd                 subrip
apng                    libzvbi_teletext        subviewer
aptx                    loco                    subviewer1
aptx_hd                 lscr                    sunrast
apv                     m101                    svq1
arbc                    mace3                   svq3
argo                    mace6                   tak
ass                     magicyuv                targa
asv1                    mdec                    targa_y216
asv2                    media100                tdsc
atrac1                  metasound               text
atrac3                  microdvd                theora
atrac3al                mimic                   thp
atrac3p                 misc4                   tiertexseqvideo
atrac3pal               mjpeg                   tiff
atrac9                  mjpeg_cuvid             tmv
aura                    mjpeg_qsv               truehd
aura2                   mjpegb                  truemotion1
av1                     mlp                     truemotion2
av1_amf                 mmvideo                 truemotion2rt
av1_cuvid               mobiclip                truespeech
av1_qsv                 motionpixels            tscc
avrn                    movtext                 tscc2
avrp                    mp1                     tta
avs                     mp1float                twinvq
avui                    mp2                     txd
bethsoftvid             mp2float                ulti
bfi                     mp3                     utvideo
bink                    mp3adu                  v210
binkaudio_dct           mp3adufloat             v210x
binkaudio_rdft          mp3float                vb
bintext                 mp3on4                  vble
bitpacked               mp3on4float             vbn
bmp                     mpc7                    vc1
bmv_audio               mpc8                    vc1_cuvid
bmv_video               mpeg1_cuvid             vc1_qsv
bonk                    mpeg1video              vc1image
brender_pix             mpeg2_cuvid             vcr1
c93                     mpeg2_qsv               vmdaudio
cavs                    mpeg2video              vmdvideo
cbd2_dpcm               mpeg4                   vmix
ccaption                mpeg4_cuvid             vmnc
cdgraphics              mpegvideo               vnull
cdtoons                 mpl2                    vorbis
cdxl                    msa1                    vp3
cfhd                    mscc                    vp4
cinepak                 msmpeg4v1               vp5
clearvideo              msmpeg4v2               vp6
cljr                    msmpeg4v3               vp6a
cllc                    msnsiren                vp6f
comfortnoise            msp2                    vp7
cook                    msrle                   vp8
cpia                    mss1                    vp8_cuvid
cri                     mss2                    vp8_qsv
cscd                    msvideo1                vp9
cyuv                    mszh                    vp9_amf
dca                     mts2                    vp9_cuvid
dds                     mv30                    vp9_qsv
derf_dpcm               mvc1                    vplayer
dfa                     mvc2                    vqa
dfpwm                   mvdv                    vqc
dirac                   mvha                    vvc
dnxhd                   mwsc                    vvc_qsv
dolby_e                 mxpeg                   wady_dpcm
dpx                     nellymoser              wavarc
dsd_lsbf                notchlc                 wavpack
dsd_lsbf_planar         nuv                     wbmp
dsd_msbf                on2avc                  wcmv
dsd_msbf_planar         opus                    webp
dsicinaudio             osq                     webp_anim
dsicinvideo             paf_audio               webvtt
dss_sp                  paf_video               wmalossless
dst                     pam                     wmapro
dvaudio                 pbm                     wmav1
dvbsub                  pcm_alaw                wmav2
dvdsub                  pcm_bluray              wmavoice
dvvideo                 pcm_dvd                 wmv1
dxa                     pcm_dvda                wmv2
dxtory                  pcm_f16le               wmv3
dxv                     pcm_f24le               wmv3image
eac3                    pcm_f32be               wnv1
eacmv                   pcm_f32le               wrapped_avframe
eamad                   pcm_f64be               ws_snd1
eatgq                   pcm_f64le               xan_dpcm
eatgv                   pcm_lxf                 xan_wc3
eatqi                   pcm_mulaw               xan_wc4
eightbps                pcm_s16be               xbin
eightsvx_exp            pcm_s16be_planar        xbm
eightsvx_fib            pcm_s16le               xface
escape124               pcm_s16le_planar        xl
escape130               pcm_s24be               xma1
evrc                    pcm_s24daud             xma2
exr                     pcm_s24le               xpm
fastaudio               pcm_s24le_planar        xsub
ffv1                    pcm_s32be               xwd
ffvhuff                 pcm_s32le               y41p
ffwavesynth             pcm_s32le_planar        ylc
fic                     pcm_s64be               yop
fits                    pcm_s64le               yuv4
flac                    pcm_s8                  zero12v
flashsv                 pcm_s8_planar           zerocodec
flashsv2                pcm_sga                 zlib
flic                    pcm_u16be               zmbv

Enabled encoders:
a64multi                hevc_d3d12va            pcm_s32be
a64multi5               hevc_mf                 pcm_s32le
aac                     hevc_nvenc              pcm_s32le_planar
aac_mf                  hevc_qsv                pcm_s64be
ac3                     hevc_vaapi              pcm_s64le
ac3_fixed               hevc_vulkan             pcm_s8
ac3_mf                  huffyuv                 pcm_s8_planar
adpcm_adx               jpeg2000                pcm_u16be
adpcm_argo              jpegls                  pcm_u16le
adpcm_g722              libaom_av1              pcm_u24be
adpcm_g726              libcodec2               pcm_u24le
adpcm_g726le            libgsm                  pcm_u32be
adpcm_ima_alp           libgsm_ms               pcm_u32le
adpcm_ima_amv           libilbc                 pcm_u8
adpcm_ima_apm           libjxl                  pcm_vidc
adpcm_ima_qt            libjxl_anim             pcx
adpcm_ima_ssi           liblc3                  pdv
adpcm_ima_wav           libmp3lame              pfm
adpcm_ima_ws            liboapv                 pgm
adpcm_ms                libopencore_amrnb       pgmyuv
adpcm_swf               libopenjpeg             phm
adpcm_yamaha            libopus                 png
alac                    librav1e                ppm
alias_pix               libshine                prores
amv                     libspeex                prores_aw
anull                   libsvtav1               prores_ks
apng                    libsvtjpegxs            prores_ks_vulkan
aptx                    libtheora               qoi
aptx_hd                 libtwolame              qtrle
apv_vulkan              libvo_amrwbenc          r10k
ass                     libvorbis               r210
asv1                    libvpx_vp8              ra_144
asv2                    libvpx_vp9              rawvideo
av1_amf                 libvvenc                roq
av1_d3d12va             libwebp                 roq_dpcm
av1_mf                  libwebp_anim            rpza
av1_nvenc               libx264                 rv10
av1_qsv                 libx264rgb              rv20
av1_vaapi               libx265                 s302m
av1_vulkan              libxavs2                sbc
avrp                    libxeve                 sgi
avui                    libxvid                 smc
bitpacked               ljpeg                   snow
bmp                     magicyuv                speedhq
cfhd                    mjpeg                   srt
cinepak                 mjpeg_qsv               ssa
cljr                    mjpeg_vaapi             subrip
comfortnoise            mlp                     sunrast
dca                     movtext                 svq1
dfpwm                   mp2                     targa
dnxhd                   mp2fixed                text
dpx                     mp3_mf                  tiff
dvbsub                  mpeg1video              truehd
dvdsub                  mpeg2_qsv               tta
dvvideo                 mpeg2_vaapi             ttml
dxv                     mpeg2video              utvideo
eac3                    mpeg4                   v210
exr                     msmpeg4v2               vbn
ffv1                    msmpeg4v3               vc2
ffv1_vulkan             msrle                   vnull
ffvhuff                 msvideo1                vorbis
fits                    nellymoser              vp8_vaapi
flac                    opus                    vp9_qsv
flashsv                 pam                     vp9_vaapi
flashsv2                pbm                     wavpack
flv                     pcm_alaw                wbmp
g723_1                  pcm_bluray              webvtt
gif                     pcm_dvd                 wmav1
h261                    pcm_f32be               wmav2
h263                    pcm_f32le               wmv1
h263p                   pcm_f64be               wmv2
h264_amf                pcm_f64le               wrapped_avframe
h264_d3d12va            pcm_mulaw               xbm
h264_mf                 pcm_s16be               xface
h264_nvenc              pcm_s16be_planar        xsub
h264_qsv                pcm_s16le               xwd
h264_vaapi              pcm_s16le_planar        y41p
h264_vulkan             pcm_s24be               yuv4
hap                     pcm_s24daud             zlib
hdr                     pcm_s24le               zmbv
hevc_amf                pcm_s24le_planar

Enabled hwaccels:
apv_vulkan              hevc_nvdec              vc1_nvdec
av1_d3d11va             hevc_nvdec_cuarray      vc1_nvdec_cuarray
av1_d3d11va2            hevc_vaapi              vc1_vaapi
av1_d3d12va             hevc_vulkan             vp8_nvdec
av1_dxva2               mjpeg_nvdec             vp8_nvdec_cuarray
av1_nvdec               mjpeg_vaapi             vp8_vaapi
av1_nvdec_cuarray       mpeg1_nvdec             vp9_d3d11va
av1_vaapi               mpeg1_nvdec_cuarray     vp9_d3d11va2
av1_vulkan              mpeg2_d3d11va           vp9_d3d12va
dpx_vulkan              mpeg2_d3d11va2          vp9_dxva2
ffv1_vulkan             mpeg2_d3d12va           vp9_nvdec
h263_vaapi              mpeg2_dxva2             vp9_nvdec_cuarray
h264_d3d11va            mpeg2_nvdec             vp9_vaapi
h264_d3d11va2           mpeg2_nvdec_cuarray     vp9_vulkan
h264_d3d12va            mpeg2_vaapi             vvc_vaapi
h264_dxva2              mpeg4_nvdec             wmv3_d3d11va
h264_nvdec              mpeg4_nvdec_cuarray     wmv3_d3d11va2
h264_nvdec_cuarray      mpeg4_vaapi             wmv3_d3d12va
h264_vaapi              prores_raw_vulkan       wmv3_dxva2
h264_vulkan             prores_vulkan           wmv3_nvdec
hevc_d3d11va            vc1_d3d11va             wmv3_nvdec_cuarray
hevc_d3d11va2           vc1_d3d11va2            wmv3_vaapi
hevc_d3d12va            vc1_d3d12va
hevc_dxva2              vc1_dxva2

Enabled parsers:
aac                     dvdsub                  mpegaudio
aac_latm                evc                     mpegvideo
ac3                     ffv1                    opus
adx                     flac                    png
ahx                     ftr                     pnm
amr                     g723_1                  prores
apv                     g729                    prores_raw
av1                     gif                     qoi
avs2                    gsm                     rv34
avs3                    h261                    sbc
bmp                     h263                    sipr
cavsvideo               h264                    tak
cook                    hdr                     vc1
cri                     hevc                    vorbis
dca                     ipu                     vp3
dirac                   jpeg2000                vp8
dnxhd                   jpegxl                  vp9
dnxuc                   jpegxs                  vvc
dolby_e                 lcevc                   webp
dpx                     misc4                   xbm
dvaudio                 mjpeg                   xma
dvbsub                  mlp                     xwd
dvd_nav                 mpeg4video

Enabled demuxers:
aa                      idcin                   pcm_f64le
aac                     idf                     pcm_mulaw
aax                     iff                     pcm_s16be
ac3                     ifv                     pcm_s16le
ac4                     ilbc                    pcm_s24be
ace                     image2                  pcm_s24le
acm                     image2_alias_pix        pcm_s32be
act                     image2_brender_pix      pcm_s32le
adf                     image2pipe              pcm_s8
adp                     image_bmp_pipe          pcm_u16be
ads                     image_cri_pipe          pcm_u16le
adx                     image_dds_pipe          pcm_u24be
aea                     image_dpx_pipe          pcm_u24le
afc                     image_exr_pipe          pcm_u32be
aiff                    image_gem_pipe          pcm_u32le
aix                     image_gif_pipe          pcm_u8
alp                     image_hdr_pipe          pcm_vidc
amr                     image_j2k_pipe          pdv
amrnb                   image_jpeg_pipe         pjs
amrwb                   image_jpegls_pipe       pmp
anm                     image_jpegxl_pipe       pp_bnk
apac                    image_jpegxs_pipe       pva
apc                     image_pam_pipe          pvf
ape                     image_pbm_pipe          qcp
apm                     image_pcx_pipe          qoa
apng                    image_pfm_pipe          r3d
aptx                    image_pgm_pipe          rawvideo
aptx_hd                 image_pgmyuv_pipe       rcwt
apv                     image_pgx_pipe          realtext
aqtitle                 image_phm_pipe          redspark
argo_asf                image_photocd_pipe      rka
argo_brp                image_pictor_pipe       rl2
argo_cvg                image_png_pipe          rm
asf                     image_ppm_pipe          roq
asf_o                   image_psd_pipe          rpl
ass                     image_qdraw_pipe        rsd
ast                     image_qoi_pipe          rso
au                      image_sgi_pipe          rtp
av1                     image_sunrast_pipe      rtsp
avi                     image_svg_pipe          s337m
avisynth                image_tiff_pipe         sami
avr                     image_vbn_pipe          sap
avs                     image_webp_pipe         sbc
avs2                    image_xbm_pipe          sbg
avs3                    image_xpm_pipe          scc
bethsoftvid             image_xwd_pipe          scd
bfi                     imf                     sdns
bfstm                   ingenient               sdp
bink                    ipmovie                 sdr2
binka                   ipu                     sds
bintext                 ircam                   sdx
bit                     iss                     segafilm
bitpacked               iv8                     ser
bmv                     ivf                     sga
boa                     ivr                     shorten
bonk                    jacosub                 siff
brstm                   jpegxl_anim             simbiosis_imx
c93                     jv                      sln
caf                     kux                     smacker
cavsvideo               kvag                    smjpeg
cdg                     laf                     smush
cdxl                    lc3                     sol
cine                    libgme                  sox
codec2                  libmodplug              spdif
codec2raw               libopenmpt              srt
concat                  live_flv                stl
dash                    lmlm4                   str
data                    loas                    subviewer
daud                    lrc                     subviewer1
dcstr                   luodat                  sup
derf                    lvf                     svag
dfa                     lxf                     svs
dfpwm                   m4v                     swf
dhav                    matroska                tak
dirac                   mca                     tedcaptions
dnxhd                   mcc                     thp
dsf                     mgsts                   threedostr
dsicin                  microdvd                tiertexseq
dss                     mjpeg                   tmv
dts                     mjpeg_2000              truehd
dtshd                   mlp                     tta
dv                      mlv                     tty
dvbsub                  mm                      txd
dvbtxt                  mmf                     ty
dvdvideo                mods                    usm
dxa                     moflex                  v210
ea                      mov                     v210x
ea_cdata                mp3                     vag
eac3                    mpc                     vc1
epaf                    mpc8                    vc1t
evc                     mpegps                  vividas
ffmetadata              mpegts                  vivo
filmstrip               mpegtsraw               vmd
fits                    mpegvideo               vobsub
flac                    mpjpeg                  voc
flic                    mpl2                    vpk
flv                     mpsub                   vplayer
fourxm                  msf                     vqf
frm                     msnwc_tcp               vvc
fsb                     msp                     w64
fwse                    mtaf                    wady
g722                    mtv                     wav
g723_1                  musx                    wavarc
g726                    mv                      wc3
g726le                  mvi                     webm_dash_manifest
g728                    mvr                     webp_anim
g729                    mxf                     webvtt
gdv                     mxg                     wsaud
genh                    nc                      wsd
gif                     nistsphere              wsvqa
gsm                     nsp                     wtv
gxf                     nsv                     wv
h261                    nut                     wve
h263                    nuv                     xa
h264                    obu                     xbin
hca                     ogg                     xmd
hcom                    oma                     xmv
hevc                    osq                     xvag
hls                     paf                     xwma
hnm                     pcm_alaw                yop
hxvs                    pcm_f32be               yuv4mpegpipe
iamf                    pcm_f32le
ico                     pcm_f64be

Enabled muxers:
a64                     h263                    pcm_s16le
ac3                     h264                    pcm_s24be
ac4                     hash                    pcm_s24le
adts                    hds                     pcm_s32be
adx                     hevc                    pcm_s32le
aea                     hls                     pcm_s8
aiff                    iamf                    pcm_u16be
alp                     ico                     pcm_u16le
amr                     ilbc                    pcm_u24be
amv                     image2                  pcm_u24le
apm                     image2pipe              pcm_u32be
apng                    ipod                    pcm_u32le
aptx                    ircam                   pcm_u8
aptx_hd                 ismv                    pcm_vidc
apv                     iterm2                  pdv
argo_asf                ivf                     psp
argo_cvg                jacosub                 rawvideo
asf                     kvag                    rcwt
asf_stream              latm                    rm
ass                     lc3                     roq
ast                     lrc                     rso
au                      m4v                     rtp
avi                     matroska                rtp_mpegts
avif                    matroska_audio          rtsp
avm2                    mcc                     sap
avs2                    md5                     sbc
avs3                    microdvd                scc
bit                     mjpeg                   segafilm
caf                     mkvtimestamp_v2         segment
cavsvideo               mlp                     smjpeg
chromaprint             mmf                     smoothstreaming
codec2                  mov                     sox
codec2raw               mp2                     spdif
crc                     mp3                     spx
dash                    mp4                     srt
data                    mpeg1system             stream_segment
daud                    mpeg1vcd                streamhash
dfpwm                   mpeg1video              sup
dirac                   mpeg2dvd                swf
dnxhd                   mpeg2svcd               tee
dts                     mpeg2video              tg2
dv                      mpeg2vob                tgp
eac3                    mpegts                  truehd
evc                     mpjpeg                  tta
f4v                     mxf                     ttml
ffmetadata              mxf_d10                 uncodedframecrc
fifo                    mxf_opatom              vc1
filmstrip               null                    vc1t
fits                    nut                     voc
flac                    obu                     vvc
flv                     oga                     w64
framecrc                ogg                     wav
framehash               ogv                     webm
framemd5                oma                     webm_chunk
g722                    opus                    webm_dash_manifest
g723_1                  pcm_alaw                webp
g726                    pcm_f32be               webvtt
g726le                  pcm_f32le               whip
gif                     pcm_f64be               wsaud
gsm                     pcm_f64le               wtv
gxf                     pcm_mulaw               wv
h261                    pcm_s16be               yuv4mpegpipe

Enabled protocols:
async                   http                    rtmp
bluray                  httpproxy               rtmpe
cache                   https                   rtmps
concat                  icecast                 rtmpt
concatf                 ipfs_gateway            rtmpte
crypto                  ipns_gateway            rtmpts
data                    librist                 rtp
dtls                    libsrt                  srtp
fd                      libssh                  subfile
ffrtmpcrypt             libzmq                  tcp
ffrtmphttp              md5                     tee
file                    mmsh                    tls
ftp                     mmst                    udp
gopher                  pipe                    udplite
gophers                 prompeg

Enabled filters:
a3dscope                dedot                   perms
aap                     deesser                 perspective
abench                  deflate                 phase
abitscope               deflicker               photosensitivity
acompressor             deinterlace_d3d12       pixdesctest
acontrast               deinterlace_qsv         pixelize
acopy                   deinterlace_vaapi       pixscope
acrossfade              dejudder                pp7
acrossover              delogo                  premultiply
acrusher                denoise_vaapi           premultiply_dynamic
acue                    deshake                 prewitt
addroi                  deshake_opencl          prewitt_opencl
adeclick                despill                 procamp_vaapi
adeclip                 detelecine              program_opencl
adecorrelate            dialoguenhance          pseudocolor
adelay                  dilation                psnr
adenorm                 dilation_opencl         pullup
aderivative             displace                qp
adrawgraph              doubleweave             qrencode
adrc                    drawbox                 qrencodesrc
adynamicequalizer       drawbox_vaapi           quirc
adynamicsmooth          drawgraph               random
aecho                   drawgrid                readeia608
aemphasis               drawtext                readvitc
aeval                   drawvg                  realtime
aevalsrc                drmeter                 remap
aexciter                dynaudnorm              remap_opencl
afade                   earwax                  removegrain
afdelaysrc              ebur128                 removelogo
afftdn                  edgedetect              repeatfields
afftfilt                elbg                    replaygain
afir                    entropy                 reverse
afireqsrc               epx                     rgbashift
afirsrc                 eq                      rgbtestsrc
aformat                 equalizer               roberts
afreqshift              erosion                 roberts_opencl
afwtdn                  erosion_opencl          rotate
agate                   estdif                  rubberband
agraphmonitor           exposure                sab
ahistogram              extractplanes           scale
aiir                    extrastereo             scale2ref
aintegral               fade                    scale_cuda
ainterleave             feedback                scale_d3d11
alatency                fftdnoiz                scale_d3d12
alimiter                fftfilt                 scale_qsv
allpass                 field                   scale_vaapi
allrgb                  fieldhint               scale_vulkan
allyuv                  fieldmatch              scdet
aloop                   fieldorder              scdet_vulkan
alphaextract            fillborders             scharr
alphamerge              find_rect               scroll
amerge                  firequalizer            segment
ametadata               flanger                 select
amf_capture             flip_vulkan             selectivecolor
amix                    flite                   sendcmd
amovie                  floodfill               separatefields
amplify                 format                  setdar
amultiply               fps                     setfield
anequalizer             framepack               setparams
anlmdn                  framerate               setpts
anlmf                   framestep               setrange
anlms                   frc_amf                 setsar
anoisesrc               freezedetect            settb
anull                   freezeframes            sharpness_vaapi
anullsink               frei0r                  shear
anullsrc                frei0r_src              showcqt
apad                    fruc_vulkan             showcwt
aperms                  fspp                    showfreqs
aphasemeter             fsync                   showinfo
aphaser                 gblur                   showpalette
aphaseshift             gblur_vulkan            showspatial
apsnr                   geq                     showspectrum
apsyclip                gfxcapture              showspectrumpic
apulsator               gradfun                 showvolume
arealtime               gradients               showwaves
aresample               graphmonitor            showwavespic
areverse                grayworld               shuffleframes
arls                    greyedge                shufflepixels
arnndn                  guided                  shuffleplanes
asdr                    haas                    sidechaincompress
asegment                haldclut                sidechaingate
aselect                 haldclutsrc             sidedata
asendcmd                hdcd                    sierpinski
asetnsamples            headphone               signalstats
asetpts                 hflip                   signature
asetrate                hflip_vulkan            silencedetect
asettb                  highpass                silenceremove
ashowinfo               highshelf               sinc
asidedata               hilbert                 sine
asisdr                  histeq                  siti
asoftclip               histogram               smartblur
aspectralstats          hqdn3d                  smptebars
asplit                  hqx                     smptehdbars
ass                     hstack                  sobel
astats                  hstack_qsv              sobel_opencl
astreamselect           hstack_vaapi            sofalizer
asubboost               hsvhold                 spectrumsynth
asubcut                 hsvkey                  speechnorm
asupercut               hue                     split
asuperpass              huesaturation           spp
asuperstop              hwdownload              sr_amf
atadenoise              hwmap                   ssim
atempo                  hwupload                ssim360
atilt                   hwupload_cuda           stereo3d
atrim                   hysteresis              stereotools
avectorscope            iccdetect               stereowiden
avgblur                 iccgen                  streamselect
avgblur_opencl          identity                subtitles
avgblur_vulkan          idet                    super2xsai
avsynctest              il                      superequalizer
axcorrelate             inflate                 surround
azmq                    interlace               swaprect
backgroundkey           interlace_vulkan        swapuv
bandpass                interleave              tblend
bandreject              join                    telecine
bass                    kerndeint               testsrc
bbox                    kirsch                  testsrc2
bench                   ladspa                  thistogram
bilateral               lagfun                  threshold
bilateral_cuda          latency                 thumbnail
biquad                  latticepal              thumbnail_cuda
bitplanenoise           lenscorrection          tile
blackdetect             lensfun                 tiltandshift
blackdetect_vulkan      libplacebo              tiltshelf
blackframe              libvmaf                 tinterlace
blend                   life                    tlut2
blend_vulkan            limitdiff               tmedian
blockdetect             limiter                 tmidequalizer
blurdetect              loop                    tmix
bm3d                    loudnorm                tonemap
boxblur                 lowpass                 tonemap_opencl
boxblur_opencl          lowshelf                tonemap_vaapi
bs2b                    lumakey                 tpad
bwdif                   lut                     transpose
bwdif_cuda              lut1d                   transpose_cuda
bwdif_vulkan            lut2                    transpose_opencl
cas                     lut3d                   transpose_vaapi
ccrepack                lutrgb                  transpose_vulkan
cellauto                lutyuv                  treble
channelmap              mandelbrot              tremolo
channelsplit            maskedclamp             trim
chorus                  maskedmax               unpremultiply
chromaber_vulkan        maskedmerge             unsharp
chromahold              maskedmin               unsharp_opencl
chromakey               maskedthreshold         untile
chromakey_cuda          maskfun                 uspp
chromanr                mcdeint                 v360
chromashift             mcompand                v360_vulkan
ciescope                median                  vaguedenoiser
codecview               mergeplanes             varblur
color                   mestimate               vectorscope
color_vulkan            mestimate_d3d12         vflip
colorbalance            metadata                vflip_vulkan
colorchannelmixer       midequalizer            vfrdet
colorchart              minterpolate            vibrance
colorcontrast           mix                     vibrato
colorcorrect            monochrome              vidstabdetect
colordetect             morpho                  vidstabtransform
colorhold               movie                   vif
colorize                mpdecimate              vignette
colorkey                mptestsrc               virtualbass
colorkey_opencl         msad                    vmafmotion
colorlevels             multiply                volume
colormap                negate                  volumedetect
colormatrix             nlmeans                 vpp_amf
colorspace              nlmeans_opencl          vpp_qsv
colorspace_cuda         nlmeans_vulkan          vqe_amf
colorspectrum           nnedi                   vstack
colortemperature        noformat                vstack_qsv
compand                 noise                   vstack_vaapi
compensationdelay       normalize               w3fdif
concat                  null                    waveform
convolution             nullsink                weave
convolution_opencl      nullsrc                 whisper
convolve                openclsrc               xbr
copy                    oscilloscope            xcorrelate
corr                    overlay                 xfade
cover_rect              overlay_cuda            xfade_opencl
crop                    overlay_opencl          xfade_vulkan
cropdetect              overlay_qsv             xmedian
crossfeed               overlay_vaapi           xpsnr
crystalizer             overlay_vulkan          xstack
cue                     owdenoise               xstack_qsv
curves                  pad                     xstack_vaapi
datascope               pad_cuda                yadif
dblur                   pad_opencl              yadif_cuda
dcshift                 pad_vaapi               yaepblur
dctdnoiz                pal100bars              yuvtestsrc
ddagrab                 pal75bars               zmq
deband                  palettegen              zoneplate
deblock                 paletteuse              zoompan
decimate                pan                     zscale
deconvolve              perlin

Enabled bsfs:
aac_adtstoasc           h264_metadata           pcm_rechunk
ahx_to_mp2              h264_mp4toannexb        pgs_frame_merge
apv_metadata            h264_redundant_pps      prores_metadata
av1_frame_merge         hapqa_extract           remove_extradata
av1_frame_split         hevc_metadata           setts
av1_metadata            hevc_mp4toannexb        showinfo
chomp                   imx_dump_header         smpte436m_to_eia608
dca_core                lcevc_merge             text2movsub
dovi_rpu                lcevc_metadata          trace_headers
dovi_split              media100_to_mjpegb      truehd_core
dts2pts                 mjpeg2jpeg              vp9_metadata
dump_extradata          mjpega_dump_header      vp9_raw_reorder
dv_error_marker         mov2textsub             vp9_superframe
eac3_core               mpeg2_metadata          vp9_superframe_split
eia608_to_smpte436m     mpeg4_unpack_bframes    vvc_metadata
evc_frame_merge         noise                   vvc_mp4toannexb
extract_extradata       null
filter_units            opus_metadata

Enabled indevs:
dshow                   lavfi                   openal
gdigrab                 libcdio                 vfwcap

Enabled outdevs:
caca

git-full external libraries' versions: 

AMF v1.5.2-2-gc35f613
aom v3.15.0-13-g441c439b99
aribb24 v1.0.3-5-g5e9be27
aribcaption 1.1.2
AviSynthPlus v3.7.5-383-g5c82777b
bs2b 3.1.0
cairo 1.18.5
chromaprint 1.6.1
codec2 1.2.0-108-g310777b1
dav1d 1.5.4-1-g52b9d3d3
davs2 1.7-1-gb41cf11
dvdnav 7.0.0-18-g8147ccd
dvdread 7.1.1-125-g0f30df5
ffnvcodec n13.1.15.0-1-geddcea9
flite v2.2-55-g6c9f20d
freetype VER-2-14-3
frei0r v3.5.0-1-ga54878c
fribidi v1.0.16-19-gc928e4c
gsm 1.0.24
harfbuzz 14.4.0-10-ge8905fb3
ladspa-sdk 1.17
lame 3.100
lc3 1.1.3
lcms2 2.16
lensfun v0.3.95-2003-g23e8cb80
libass 0.17.5-8-gb2fe9d8
libbluray 1.5.0-29-gea3e318
libcaca 0.99.
libcdio-paranoia 10.2
libgme 0.6.6
libilbc v3.0.4-346-g6adb26d4a4
libjxl v0.12-snapshot-9-gaea3a06e
libmodplug 0.8.9.0
libopencore-amrnb 0.1.6
libopencore-amrwb 0.1.6
libplacebo v7.360.0-118-g41ac298
libsoxr 0.1.3
libssh 0.12.0
libtheora v1.2.0
libwebp v1.6.0-245-g9c4a699
openal-soft 1.25.2-48-g7532541
openapv v0.3.0.0-9-ga5312e4
OpenCL-Headers 
openjpeg2 2.5.4
openmpt libopenmpt-0.6.30-3-g6c0a06bc
opus v1.6.1-53-g228a0f85
qrencode 4.1.1
quirc 1.2
rav1e p20250624-5-ge71e03a
rist 0.2.20
rubberband v4.0.0
SDL release-2.32.0-231-g5882a4f13
shine 3.1.1
snappy 1.2.2
speex Speex-1.2.1-51-g0589522
srt v1.5.7
SVT-AV1 v4.2.0-cqp-extended-27-g069628233
SVT-JPEG-XS v0.9.0-94-g96f8f06
twolame 0.4.0
uavs3d 1.2
VAAPI 2.25.0.
vidstab v1.1.2-214-ge2445c4
vmaf v3.2.0-13-gf85a8536
vo-amrwbenc 0.1.3
vorbis v1.3.7-37-g1b75110b
VPL 2.17
vpx v1.17.0-56-g5c880e08d
vulkan-loader v1.4.361-3-gb1d75f38
vvenc v1.14.0-168-g0a41b11
whisper.cpp 1.9.3
x264 v0.165.3223
x265 4.3-22-g98eec40
xavs2 1.4
xevd 0.7.0
xeve 0.7.0
xvid v1.3.7
zeromq 4.3.5
zimg release-3.0.6-252-gf6cc75a
zvbi v0.2.45

