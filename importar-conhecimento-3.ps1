$baseUrl = "http://localhost:5205/conhecimento"

$dados = @(

@{
    conteudo = "A tecnologia bioflocos (BFT) permite alta densidade de cultivo com mínima troca de água, utilizando microrganismos para reciclagem de nutrientes dentro do próprio sistema."
    tags = "bft,conceito,bioflocos,sistema intensivo,reciclagem nutrientes"
    clienteId = 1
},
@{
    conteudo = "Os bioflocos se formam naturalmente em sistemas com alta densidade, baixa renovação de água e forte aeração."
    tags = "formacao bioflocos,condicoes sistema,aeracao,densidade"
    clienteId = 1
},
@{
    conteudo = "O sistema bioflocos tem origem no processo de lodo ativado, utilizado no tratamento biológico de efluentes."
    tags = "origem bioflocos,lodo ativado,tratamento efluentes"
    clienteId = 1
},
@{
    conteudo = "A reciclagem de nutrientes no bioflocos ocorre dentro do próprio tanque, reduzindo a necessidade de filtros externos."
    tags = "reciclagem nutrientes,bioflocos vs ras,filtros"
    clienteId = 1
},
@{
    conteudo = "Os bioflocos são compostos por microrganismos como bactérias, microalgas, protozoários e matéria orgânica em suspensão."
    tags = "composicao bioflocos,microorganismos,estrutura biofloco"
    clienteId = 1
},
@{
    conteudo = "A tecnologia bioflocos permite maior controle da qualidade da água, aumentando produtividade e biossegurança no cultivo."
    tags = "vantagens bioflocos,produtividade,biosseguranca,qualidade agua"
    clienteId = 1
},
@{
    conteudo = "Sistemas BFT operam com baixa ou nenhuma troca de água, reduzindo impacto ambiental e consumo hídrico."
    tags = "sustentabilidade,bioflocos,baixo consumo agua"
    clienteId = 1
},
@{
    conteudo = "A principal fonte de nitrogênio no sistema é a ração, sendo grande parte convertida em amônia pelos organismos cultivados."
    tags = "nitrogenio,racao,amonia,metabolismo peixe"
    clienteId = 1
},
@{
    conteudo = "A amônia no sistema pode ser assimilada por microalgas, bactérias heterotróficas e bactérias nitrificantes."
    tags = "amonia controle,microalgas,heterotroficas,nitrificantes"
    clienteId = 1
},
@{
    conteudo = "O ciclo do carbono no bioflocos envolve carbono orgânico e inorgânico, ambos essenciais para o equilíbrio do sistema."
    tags = "ciclo carbono,carbono organico,inorganico,bioflocos"
    clienteId = 1
},
@{
    conteudo = "O carbono orgânico é utilizado por bactérias heterotróficas como fonte de energia para crescimento e assimilação de nitrogênio."
    tags = "carbono organico,bacterias heterotroficas,energia sistema"
    clienteId = 1
},
@{
    conteudo = "O carbono inorgânico, medido pela alcalinidade, é essencial para o funcionamento das bactérias nitrificantes."
    tags = "carbono inorganico,alcalinidade,nitrificacao"
    clienteId = 1
},
@{
    conteudo = "A alcalinidade atua como sistema tampão da água e deve ser monitorada para garantir estabilidade do pH."
    tags = "alcalinidade,ph,tampao qualidade agua"
    clienteId = 1
},
@{
    conteudo = "A manutenção da alcalinidade é essencial para permitir a oxidação da amônia pelas bactérias nitrificantes."
    tags = "alcalinidade nitrificacao,controle amonia"
    clienteId = 1
},
@{
    conteudo = "O sistema bioflocos exige monitoramento constante de parâmetros como pH, temperatura, alcalinidade e sólidos suspensos."
    tags = "monitoramento,qualidade agua,parametros bft"
    clienteId = 1
},
@{
    conteudo = "A alta concentração de sólidos suspensos é característica do bioflocos e pode ser tolerada por espécies filtradoras."
    tags = "solidos suspensos,especies filtradoras,bioflocos"
    clienteId = 1
},
@{
    conteudo = "Espécies ideais para bioflocos devem tolerar altas densidades, sólidos suspensos e compostos nitrogenados."
    tags = "especies bft,tilapia,caracteristicas ideais"
    clienteId = 1
},
@{
    conteudo = "Peixes onívoros e filtradores têm maior capacidade de aproveitar os bioflocos como alimento natural."
    tags = "alimentacao bioflocos,peixes onivoros,filtradores"
    clienteId = 1
},
@{
    conteudo = "O sistema Bio-RAS combina bioflocos com recirculação de água, buscando maior eficiência e redução de custos."
    tags = "bio ras,recirculacao,integracao sistemas"
    clienteId = 1
},
@{
    conteudo = "A tecnologia bioflocos reduz a necessidade de biofiltros externos, pois o tratamento ocorre no próprio tanque."
    tags = "biofiltro,bioflocos,tratamento interno"
    clienteId = 1
},
@{
    conteudo = "A baixa troca de água no bioflocos contribui para maior estabilidade térmica e ambiental no cultivo."
    tags = "estabilidade temperatura,baixa troca agua"
    clienteId = 1
},
@{
    conteudo = "O sistema bioflocos permite operação com tanques independentes, aumentando controle e biossegurança."
    tags = "controle sistema,biosseguranca,tanques independentes"
    clienteId = 1
},
@{
    conteudo = "A tecnologia bioflocos possibilita alta produtividade com menor uso de área comparado a sistemas tradicionais."
    tags = "produtividade alta,area reduzida,bft"
    clienteId = 1
},
@{
    conteudo = "Entre as desvantagens do bioflocos estão o alto custo inicial, necessidade de energia e mão de obra especializada."
    tags = "desvantagens bft,custo energia,operacao"
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