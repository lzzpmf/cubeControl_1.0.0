# CubeSuite Android — acervo extraído do APK

[Engenharia reversa](../README.md) · [Análise de protocolo](../../CubeControl-1.1/Cube/docs/historico/analise-protocolo-windows.md)

Esta pasta identifica o material Android usado na engenharia reversa: código descompilado e recursos extraídos do CubeSuite, pacote `com.cubeSuite`. O arquivo APK original e um projeto Android compilável não estão incluídos neste acervo.

| Pasta | Conteúdo |
| --- | --- |
| [sources/](sources/) | Fontes Java descompiladas, código do CubeSuite e dependências incorporadas |
| [resources/](resources/) | Recursos Android, assets e metadados de bibliotecas extraídos |

Pontos de entrada da investigação:

- [Communication.java](sources/com/cubeSuite/communication/Communication.java): construção de pacotes e operações genéricas.
- [CommunicationUtil.java](sources/com/cubeSuite/communication/CommunicationUtil.java): transporte BLE e respostas.
- [CubeBaby.java](sources/com/cubeSuite/entity/Instrument/CubeBaby.java): classe vazia neste acervo; não fornece o mapa específico do pedal.
- [README do jsoup](resources/META-INF/README.md): documento de terceiro, preservado integralmente.

Trechos descompilados podem conter reconstruções incompletas ou inválidas. O suporte BLE de outros produtos do CubeSuite não comprova suporte equivalente no Cube Baby. Consulte a análise e o [catálogo de comandos](../cube-baby/CUBE_BABY_COMANDOS.md) para conhecer o alcance de cada evidência.

`sources/` e `resources/` foram movidas juntas da raiz, preservando seus conteúdos. A reorganização não recompila o APK nem altera seus recursos.
