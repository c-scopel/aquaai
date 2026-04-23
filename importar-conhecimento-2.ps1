$baseUrl = "http://localhost:5205/conhecimento"

$dados = @(

@{
    conteudo = "O sistema de bioflocos opera com alta densidade de estocagem, baixa ou nenhuma troca de água e uso contínuo de aeração."
    tags = "bioflocos,bft,sistema intensivo,alta densidade,pouca troca de agua,aeracao 24h"
    clienteId = 1
},
@{
    conteudo = "Bioflocos são agregados formados por bactérias, microalgas, protozoários, fezes e restos de ração, que reciclam nutrientes no sistema."
    tags = "bioflocos,o que é biofloco,formacao biofloco,microorganismos,aquicultura"
    clienteId = 1
},
@{
    conteudo = "Os bioflocos funcionam como alimento natural suplementar, melhorando a conversão alimentar e a eficiência produtiva."
    tags = "biofloco alimento,conversao alimentar,nutricao peixe,eficiencia cultivo"
    clienteId = 1
},
@{
    conteudo = "O acúmulo de ração e excretas aumenta compostos nitrogenados como amônia e nitrito, que devem ser controlados no sistema."
    tags = "amonia,nitrito,compostos nitrogenados,residuos racao,qualidade agua"
    clienteId = 1
},
@{
    conteudo = "A nitrificação é o processo biológico que converte amônia em nitrito e depois em nitrato, reduzindo a toxicidade da água."
    tags = "nitrificacao,amonia nitrito nitrato,processo biologico,qualidade agua"
    clienteId = 1
},
@{
    conteudo = "A adição de carbono orgânico estimula bactérias heterotróficas que consomem amônia e reduzem sua concentração na água."
    tags = "carbono organico,controle amonia,bacterias heterotroficas,melaco,fonte carbono"
    clienteId = 1
},
@{
    conteudo = "A relação prática indica que cerca de 6g de carbono orgânico são necessários para neutralizar 1g de amônia no sistema."
    tags = "relacao carbono nitrogenio,controle amonia,dosagem carbono,bioflocos"
    clienteId = 1
},
@{
    conteudo = "O excesso de carbono orgânico aumenta os sólidos na água e eleva a demanda por oxigênio e aeração."
    tags = "excesso carbono,solidos altos,demanda oxigenio,aeracao custo"
    clienteId = 1
},
@{
    conteudo = "Sólidos em excesso são o principal fator limitante em sistemas de bioflocos e devem ser controlados continuamente."
    tags = "solidos bioflocos,controle solidos,limitante sistema,qualidade agua"
    clienteId = 1
},
@{
    conteudo = "Os sólidos devem ser monitorados e mantidos em níveis adequados para evitar aumento de amônia e problemas no cultivo."
    tags = "solidos monitoramento,qualidade agua,controle biofloco"
    clienteId = 1
},
@{
    conteudo = "O dreno central é utilizado para concentrar e remover sólidos como fezes, restos de ração e bioflocos do viveiro."
    tags = "dreno,toalete camarao,remocao solidos,manejo viveiro"
    clienteId = 1
},
@{
    conteudo = "A frequência de drenagem aumenta com o crescimento da biomassa e maior oferta de ração no sistema."
    tags = "drenagem frequencia,biomassa alta,alimentacao manejo"
    clienteId = 1
},
@{
    conteudo = "Sedimentadores são utilizados para remover sólidos da água e evitar acúmulo de matéria orgânica no sistema."
    tags = "sedimentador,remocao solidos,tratamento agua bioflocos"
    clienteId = 1
},
@{
    conteudo = "A água tratada deve retornar ao viveiro após passagem pelo sedimentador, mantendo o sistema com baixa renovação de água."
    tags = "recirculacao agua,sedimentador bioflocos,baixa troca agua"
    clienteId = 1
},
@{
    conteudo = "A alcalinidade deve ser mantida entre 120 e 180 mg/L para garantir o funcionamento das bactérias nitrificantes."
    tags = "alcalinidade ideal,controle ph,nitrificacao,bioflocos"
    clienteId = 1
},
@{
    conteudo = "A alcalinidade deve ser corrigida com bicarbonato, calcário ou carbonatos sempre que cair abaixo de 120 mg/L."
    tags = "corrigir alcalinidade,bicarbonato,calcario,ph baixo"
    clienteId = 1
},
@{
    conteudo = "A adição de alcalinizantes deve ser feita de forma fracionada para evitar variações bruscas de pH."
    tags = "controle ph,variacao ph,alcalinidade manejo"
    clienteId = 1
},
@{
    conteudo = "O sistema bioflocos depende de comunidades microbianas que atuam na reciclagem de nutrientes e controle da qualidade da água."
    tags = "microbiologia bioflocos,bacterias,microalgas,qualidade agua"
    clienteId = 1
},
@{
    conteudo = "Existem três vias principais no bioflocos: microalgas, bactérias heterotróficas e bactérias nitrificantes."
    tags = "vias bioflocos,microalgas,heterotroficas,nitrificantes"
    clienteId = 1
},
@{
    conteudo = "A reutilização de água de cultivos anteriores acelera a formação de bioflocos, mas aumenta o risco de doenças."
    tags = "reuso agua,biosseguranca,doencas bioflocos"
    clienteId = 1
},
@{
    conteudo = "O uso de substratos como bioballs e fibras aumenta a fixação de bactérias nitrificantes e melhora o controle de nitrito."
    tags = "substrato biologico,bioballs,biofilme,nitrificacao"
    clienteId = 1
},
@{
    conteudo = "O tratamento prévio da água é essencial para evitar entrada de patógenos, predadores e organismos indesejados."
    tags = "tratamento agua,biosseguranca,pre filtracao,desinfeccao"
    clienteId = 1
},
@{
    conteudo = "Sistemas de bioflocos possuem alta dependência de energia elétrica devido à necessidade contínua de aeração."
    tags = "energia bioflocos,aeracao continua,risco energia"
    clienteId = 1
},
@{
    conteudo = "A interrupção da aeração por poucos minutos pode causar perdas totais no cultivo em sistemas intensivos."
    tags = "falha aeracao,risco mortalidade,emergencia cultivo"
    clienteId = 1
}


)

foreach ($item in $dados) {

    $json = $item | ConvertTo-Json -Depth 3

    try {
        Invoke-RestMethod -Uri $baseUrl `
            -Method Post `
            -ContentType "application/json" `
            -Body $json

        Write-Host "OK: $($item.conteudo.Substring(0,40))..."
    }
    catch {
        Write-Host "ERRO ao enviar item"
    }
}