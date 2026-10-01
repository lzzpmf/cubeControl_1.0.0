# Catálogo de amps e gabinetes

[Documentação](README.md) · Revisão: 01/10/2026

Esta página separa **índices do protocolo**, **aliases do pedal** e **referências visuais da interface**. As associações a amplificadores comerciais não foram confirmadas por identificação retornada do aparelho nem por comparação acústica. As antigas estrelas e porcentagens de certeza foram removidas por não terem método de validação documentado.

## Amplificadores

TYPE USB usa índices 0–8; as posições apresentadas ao usuário são 1–9. Categorias do pedal seguem a tabela do manual transcrita na [investigação dos amps](../../../engenharia-reversa/cube-baby/CUBE_BABY_AMPS.md).

| Posição | TYPE | Alias no perfil | Categoria do pedal | Referência visual local |
| ---: | ---: | --- | --- | --- |
| 1 | 0 | Power-Zone Clean | Clean | Koch Powertone |
| 2 | 1 | US Gold 100 Clean | Clean | Friedman BE-100 |
| 3 | 2 | Two Stone Coral OD | Overdrive | Two-Rock Coral |
| 4 | 3 | Doctor3 B | Overdrive | Dr. Z MAZ 38 |
| 5 | 4 | Cali JP A | Overdrive | Mesa/Boogie JP-2C |
| 6 | 5 | Day Tripper OD | Distortion | Vox AC30 |
| 7 | 6 | Shittcow Dist | Distortion | VHT Pittbull |
| 8 | 7 | Wo Stone Coral OD | Distortion | Two-Rock Coral / variante; associação não confirmada |
| 9 | 8 | Mr Smith Dist | Distortion | PRS Archon |

**Divergência no código:** `EquipmentCatalog` classifica Day Tripper como Overdrive, enquanto `CubeBabyProfile` e a tabela do manual usam Distortion. Isso pode afetar a categoria/filtro visual. O texto de `EquipmentCatalog` também conserva uma porcentagem de confiança para Wo Stone; ela não deve ser entendida como probabilidade medida. A revisão documental não modificou o catálogo C#.

B, A e OD em aliases não comprovam canais com esses nomes nos equipamentos comerciais apresentados.

## Gabinetes / IRs

`cabinet=0` é OFF e não possui setor. Valores 1–8 selecionam slots internos 0–7. A posição só indica onde os dados estão; a identificação de fábrica depende do conteúdo reconhecido.

| Valor de cabinet | Slot interno | Alias/referência do catálogo |
| ---: | ---: | --- |
| 0 | — | OFF / sem gabinete |
| 1 | 0 | Line 6 Vetta 2×12 |
| 2 | 1 | Marshall 1960AV 4×12 |
| 3 | 2 | Marshall 1960A T75 LA610 4×12 |
| 4 | 3 | VHT/Fryette Deliverance 2×12 |
| 5 | 4 | Soldano 2×12 |
| 6 | 5 | Peavey 5150 + Mesa Oversized 4×12 |
| 7 | 6 | JSX KT77 + Mesa Stiletto AKG C1000 4×12 |
| 8 | 7 | Diezel V30 SM57 (Soldano HR PowerAmp) 4×12 |

Os nomes de microfones/power amps fazem parte dos aliases históricos; não são metadados medidos do IR. LA610 permanece uma interpretação de cadeia de captura. Não inferir falantes do Soldano nem ano/modelo exato apenas pela foto.

Para o modelo comercial **Marshall 1960AV**, a referência do fabricante usa **G12-Vintage**, conforme [características dos gabinetes](cabinet-characteristics.md). A descrição antiga “Vintage 30” ainda está em `EquipmentCatalog.cs` e precisa de uma correção de código separada.

## Fontes e limites

[Perfil USB](../windows/Cube.Protocol/CubeBabyProfile.cs), [catálogo de apresentação](../windows/Cube.Protocol/EquipmentCatalog.cs), [identificação de gabinetes](../windows/Cube.Protocol/CabinetCatalog.cs) e [imagens](../windows/Cube.Desktop/Assets/Equipment/README.md).

Características de gabinete descrevem um modelo de referência, não uma medição do arquivo. Setores personalizados ou desconhecidos não devem herdar automaticamente identidade e foto de fábrica.
