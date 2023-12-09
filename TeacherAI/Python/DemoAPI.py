import os
from langchain.llms import OpenAI
from langchain.prompts import PromptTemplate
from langchain.chains import ConversationChain
import os
os.environ['OPENAI_API_KEY'] = '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'

conversations = []

res_gen = " Fotocintezė yra gamtos procesas, kuris naudoja saulės spindulius organizmui fotosintezės metu gauti energijos. Fotosintezės metu augalai naudoja saulės spindulius, vandenį ir anglies dioksidą, kad sukurtų angliavandenilius, tokius kaip cukrus. Anglies dioksidas konvertuojamas į angliavandenilius, o vanduo paverčiamas į deguonį. Saulės spinduliai yra energijos šaltinis, kuris stimuliuoja fotosintezę, kadangi be šios energijos šis procesas negalėtų vykti."
res_tol = "  Be saulės spindulių, anglies dioksidas ir vanduo taip pat yra svarbūs fotosintezės procesui. Anglies dioksidas konvertuojamas į angliavandenilius, o vanduo paverčiamas į deguonį. Fotosintezė yra svarbus procesas augalams, nes jis leidžia augalams gauti energiją, kurios jie reikalauja augimui. Be to, fotosintezė taip pat gamina deguonį, kuris yra būtinas kitiems gyvūnams."
res_klow = " \n{\"klausimas\": \"Kokia yra optimali saulės spindulių koncentracija fotosintezės procesui?\", \"atsakymai\": {\"A\": \"Per daug\", \"B\": \"Per mažai\", \"C\": \"Šviesos spinduliai\", \"D\": \"Optimali\"}, \"teisingas_atsakymas\": \"D\"}"
res_ats = "Taip, atsakymas D yra teisingas. Fotosintezė reikalauja optimalių saulės spindulių koncentracijos, kadangi per mažai arba per daug saulės spindulių gali pažeisti augalų fotosintezės procesą. Šviesos spinduliai gali būti naudojami kaip papildomas energijos šaltinis fotosintezės procesui stimuliuoti, tačiau optimali saulės spindulių koncentracija yra būtina norint palaikyti sveikus augalus."

templatas = PromptTemplate(
    input_variables=['subject', 'scope', 'topic'],
    template="Tu esi mokytojas robotas, kuris duoda informacijos astuntokui. Dabar papasakok astuntokui apie is {subject} -> {scope} temos apie {topic}. Informacija turi buti lengvai suprantama ir nebendrauk su mokyniu;"
)

templatas_atsakymo = PromptTemplate(
    input_variables=['atsakymas'],
    template="As manau, kad atsakymas yra {atsakymas}. Pasakyk ar gerai as atsakiau. Jei blogai paiskink koks turėjo buti atsakymas ir kodėl šitas blogas."
)

template_toliau = 'Tesk pasakoti apie temą'

template_klausimas = 'Is tavo duotos informacijos sugeneruok 1 klausimą su 4 atsakymais A B C D iskuriu tik vienas tesingas, o 3 neteisingi. Informacija pateik tokiu json formatu {' \
                      '"klausimas": "", ' \
                      '"atsakymai": {' \
                      '"A": "", ' \
                      '"B": " ", ' \
                      '"C": "", ' \
                      '"D": ""' \
                      '}, ' \
                      '"teisingas_atsakymas": ""' \
                      '}'

# Import the Flask module
from flask import Flask, request, jsonify

# Create a Flask app object
app = Flask(__name__)

# Define a controller function for the endpoint that takes a body object
@app.route('/generuok/<int:conversation_id>', methods=['POST'])
def controller(conversation_id):
    if(conversation_id>len(conversations)):
        return jsonify({'message': 'Bad params'}), 400
    body = request.get_json()
    # Validate the body object
    if not body or not isinstance(body, dict):
        return jsonify({'error': 'Invalid body object'}), 400
    # Get the subject, scope and topic from the body object
    subject = body.get('subject')
    scope = body.get('scope')
    topic = body.get('topic')
    # Validate the subject, scope and topic
    if not subject or not scope or not topic or not isinstance(subject, str) or not isinstance(scope,
                                                                                               str) or not isinstance(
            topic, str):
        return jsonify({'error': 'Invalid subject, scope or topic'}), 400
    # Generate a prompt using the templatas object and the subject, scope and topic

    prompt = templatas.format(subject=subject, scope=scope, topic=topic)
    print(prompt)

    return jsonify({'response': res_gen}), 200


# Define a controller function for the endpoint that gives more information
@app.route('/toliau/<int:conversation_id>', methods=['GET'])
def toliau(conversation_id):
    if(conversation_id>len(conversations)):
        return jsonify({'message': 'Bad params'}), 400
    return jsonify({'response': res_tol }), 200


@app.route('/klausimas/<int:conversation_id>', methods=['GET'])
def klausimas(conversation_id):
    if(conversation_id>len(conversations)):
        return jsonify({'message': 'Bad params'}), 400
    return jsonify({'response': res_klow}), 200

@app.route('/atsakymas/<int:conversation_id>', methods=['POST'])
def atsakymas(conversation_id):
    if(conversation_id>len(conversations)):
        return jsonify({'message': 'Bad params'}), 400
    body = request.get_json()
    if not body or not isinstance(body, dict):
        return jsonify({'error': 'Invalid body object'}), 400
    answ = body.get('answer')
    prompt = templatas_atsakymo.format(atsakymas=answ)
    print(prompt)
    return jsonify({'response': res_ats}), 200

@app.route('/create', methods=['POST'])
def add_conversation():
    convo = ConversationChain(llm=OpenAI(temperature=0.7))
    conversations.append(convo)
    return jsonify({'id': len(conversations)-1}), 201

# Run the app
if __name__ == '__main__':
    app.run(debug=True)
