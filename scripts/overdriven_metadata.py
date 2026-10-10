"""Reviewed naming metadata. Sources are recorded per collection; generation is offline.

Cabinet brands inferred across capture generations remain probable. Do not infer
speaker brands or microphone models from an undocumented abbreviation.
"""
BASE = 'https://overdriven.fr/overdriven/index.php/'
CONVENTION = BASE + 'irdownloads/'
ENGL = BASE + 'german-112-celestion-g12h55-home-studio-edition-free-guitar-cab-impulse-responses-download/'
MESA = BASE + 'blog-posts-2/'

# key: brand, layout, cabinet, speaker, confidence, page slug, evidence
COLLECTIONS = {
    '212-GOV': ('Zilla', '2x12', 'Fatboy', 'Eminence Governor', 'confirmed',
        'uk212-gov-home-studio-edition-free-guitar-cab-impulse-responses-download/',
        'O readme identifica FB e Governor; a convenção oficial identifica FB como Zilla Fatboy.'),
    'DE-112-T75-G4-CLASSD': ('Engl', '1x12', 'E112 / DE-112', 'T-75', 'probable',
        'de-112-t-75-g4-free-guitar-cab-impulse-responses-download/', ''),
    'E112-BVV': ('Engl', '1x12', 'E112 / DE-112', 'BV30V', 'probable',
        'de-112-bv30v-g4-free-guitar-cab-impulse-responses-download/', ''),
    'E112-CV-CLASSD': ('Engl', '1x12', 'E112 / DE-112', 'CV', 'probable',
        'de-112-cv-g4-free-guitar-cab-impulse-responses-download/', ''),
    'E112-GM-G4': ('Engl', '1x12', 'E112 / DE-112', 'Grey Mojo (GM)', 'probable',
        'de-112-gm-g4-free-guitar-cab-impulse-responses-download/', ''),
    'E112-K100-G4': ('Engl', '1x12', 'E112 / DE-112', 'K-100', 'probable',
        'e112-k-100-free-guitar-cab-impulse-responses-download/', ''),
    'E112-P50E-CLASSD': ('Engl', '1x12', 'E112 / DE-112', 'P50E', 'probable',
        'e112-p50-home-studio-edition-free-guitar-cab-impulse-responses-download/', ''),
    'E112-VT-CLASSD': ('Engl', '1x12', 'E112 / DE-112', 'V-Type (VT)', 'probable',
        'de-112-vt-g4-free-guitar-cab-impulse-responses-download/', ''),
    'FB-VET30-G4': ('Zilla', '2x12', 'Fatboy', 'WGS Veteran 30 (VET30)', 'confirmed',
        'fb-212-vet30-g4-free-guitar-cab-impulse-responses-download/',
        'O readme identifica FATBOY-212 e WGS VET30; a convenção oficial identifica FB como Zilla Fatboy.'),
    'M212-GB55-CLASSD': ('Marshall', '2x12', 'M-212', 'Greenback 55 Hz (GB55)', 'probable',
        'm-212-gb55-g4-free-guitar-cab-impulse-responses-download/', ''),
    'M212-P50': ('Marshall', '2x12', 'M-212', 'P50', 'probable',
        'm212-p50-g4-free-guitar-cab-impulse-responses-download/', ''),
    'M212-VINT-L-G4-CLASSD': ('Marshall', '2x12', 'M-212', 'G12 Vintage · falante esquerdo', 'probable',
        'm-212-l-vint-g4-free-guitar-cab-impulse-responses-download/', ''),
    'R112-V30-G4': ('Mesa Boogie', '1x12', 'Rectifier', 'Celestion Vintage 30 · Mesa specs', 'confirmed',
        'r-112-v30-g4-free-guitar-cab-impulse-responses-download/',
        'O readme identifica RECTO-112; o anúncio oficial de 18/10/2025 identifica Mesa Recto 1x12 e V30 UK.'),
}

# Published "inspired by" labels, not assertions that the physical mic is that model.
MICROPHONES = {
    'DYN-906': 'Sennheiser E906', 'DYN-421': 'Sennheiser MD421',
    'DYN-201': 'Beyerdynamic M201TG', 'DYN-75': 'T.Bone MB75',
    'DYN-85B': 'T.Bone MB85B', 'DYN-75B': 'T.Bone MB75B',
    'DYN-57': 'Shure SM57', 'DYN-58': 'Shure SM58', 'DYN-7B': 'Shure SM7B',
    'DYN-I5': 'Audix I5', 'DYN-R20': 'Heil Sound PR20', 'DYN-628': 'Superlux PRA628MKII',
    'CND-NT1': 'RODE NT1-A', 'CND-2020': 'Audio Technica AT2020',
    'RBN-160': 'Beyerdynamic M160', 'CND-M3': 'RODE M3',
    'DYN-US-8': 'Electro Voice RE-20', 'CND-JP-1': 'Audio Technica AE3000',
}
# Additional equivalences are scoped to the collection page that documents them.
MICROPHONE_OVERRIDES = {
    '212-GOV': {'RBN-US-1': 'Royer R-10'},
    'M212-VINT-L-G4-CLASSD': {'RBN-US-1': 'Royer R-10'},
    'FB-VET30-G4': {
        'CND-AU-1': 'Lewitt LCT-440', 'DN-409U': 'Sennheiser MD409 U3',
        'DN-421B': 'Sennheiser MD421 II', 'DN-57': 'Shure SM57',
        'LD-103': 'Neumann TLM 103', 'LD-67NOS': 'Neumann U67 (NOS)',
        'LD-800': 'Sony C800G', 'RBN-US-1': 'Royer R-10',
    },
    'R112-V30-G4': {
        'DN-421B': 'Sennheiser MD421 II', 'DN-57': 'Shure SM57',
        'LD-12': 'AKG C12', 'LD-103': 'Neumann TLM 103',
        'LD-414B': 'AKG C414 (CK12 Brass)', 'LD-414US': 'AKG C414 (CK12 Nylon)',
        'LD-800': 'Sony C800G', 'LD-87M': 'Neumann U87Ai', 'LD-87V': 'Neumann U87i',
        'RB-121': 'Royer R-121',
    },
}


def classification(key):
    brand, layout, cabinet, speaker, confidence, slug, evidence = COLLECTIONS[key]
    sources = [BASE + slug, CONVENTION]
    if brand == 'Engl':
        evidence = ('Identificação provável por continuidade do código E112/German-112: '
                    'o autor identifica a geração anterior como Engl 112 VB; a página desta coleção não declara a marca.')
        sources.append(ENGL)
    if brand == 'Marshall':
        evidence = ('Identificação provável pelo código M-212, gabinete britânico vintage e referência M. G12 Vintage '
                    'nos readmes da família; a documentação não confirma fabricante nem modelo comercial.')
    if brand == 'Mesa Boogie':
        sources.append(MESA)
    if key == 'E112-CV-CLASSD':
        evidence += ' A página oficial identifica CV; o readme repete V-Type. CV e VT são mantidos separados.'
    return dict(Brand=brand, SpeakerConfiguration=layout, CabinetModel=cabinet,
                SpeakerLabel=speaker, Identification=confidence, IdentificationEvidence=evidence,
                Sources=sources)
