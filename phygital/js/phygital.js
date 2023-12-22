/**/
(function ($, Drupal, drupalSettings) {
    let onlyOnce;
    Drupal.behaviors.viewtipologie = {
        attach: function (context, settings) {
            const dev_root = "https://dev.phygital.bbsitalia.com";
            const prod_root = "https://testnew.visitgenoa.it";
            const env_root = location.origin;
            if (!onlyOnce) {
                onlyOnce = true;
                $(document, context)
                    .once("viewtipologie")
                    .each(function () {
                        /*
                         * INIZIO CODICE JAVASCRIPT
                         */
                        poi_data_gl = sessionStorage.getItem("glJson");
                        console.log(poi_data_gl);
                        sferiche = sessionStorage.getItem("sferiche");
                        jsonUrl = env_root + "/sites/default/files/sferiche/sferiche.json";
                        if (sferiche == null) {
                            console.log("sferiche NON è settata");
                            getSferiche(jsonUrl);
                        } else {
                            if (poi_data_gl == null) {
                                console.log("la variabile di sessione NON è settata");
                                getJsonPoi(prod_root + "/jsonapi/api-poi-list");
                            } else {
                                console.log("la variabile di sessione è settata");
                                var map = L.map("map").setView([44.4, 8.93], 16);
                                L.tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
                                    maxZoom: 19,
                                    //attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
                                }).addTo(map);
                                const data_json = JSON.parse(poi_data_gl);

                                // Itero attraverso gli oggetti presenti nell'oggetto originale
                                for (const id in data_json) {
                                    const oggetto = data_json[id];
                                    const coordinateString = oggetto.coordinate;
                                    const match = coordinateString.match(/POINT \((\S+) (\S+)\)/);
                                    lng = "";
                                    lat = "";
                                    if (match) {
                                        lng = parseFloat(match[1]);
                                        lat = parseFloat(match[2]);
                                        const popupContent = `
                                            <b>${oggetto.nome}</b><br>
                                            Coordinate: ${lat}, ${lng}<br>
                                            Altri dettagli qui...<br>
                                            // Puoi aggiungere ulteriori dettagli qui se necessario`;
                                        L.marker([lat, lng]).addTo(map).bindPopup(popupContent).openPopup();
                                    }

                                    // Ora puoi accedere ai campi di ogni oggetto

                                    // E così via per gli altri campi che desideri stampare o utilizzare
                                }

                                jsonDataString = sessionStorage.getItem("sferiche");
                                console.log("ECCOMI");
                                console.log(jsonDataString);
                                jsonData = JSON.parse(jsonDataString);
                                const redIcon = L.icon({
                                    iconUrl: window.location.origin + "/modules/custom/phygital/files/marker-icon-2x-red.png",
                                    iconSize: [25, 41],
                                    iconAnchor: [12, 41],
                                    popupAnchor: [1, -34],
                                    shadowSize: [41, 41],
                                });
                                console.log(jsonData);
                                jsonData.slice(0, 22000).forEach((item) => {
                                    const lat = parseFloat(item.Y);
                                    const lng = parseFloat(item.X);

                                    // Creo una stringa di template per il contenuto della popup
                                    const popupContent = `
                                        <b>ID:</b> ${item.Id}<br>
                                        <b>Filename:</b> ${item.Filename}<br>`;

                                    // Creiamo il marker rosso personalizzato
                                    const marker = L.marker([lat, lng], { icon: redIcon });

                                    // Aggiungiamo la popup al marker
                                    marker.bindPopup(popupContent);

                                    // Aggiungiamo il marker alla mappa
                                    marker.addTo(map);
                                });
                            }
                        }

                        console.log("leaflet map");

                        /*
                         * FINE CODICE JAVASCRIPT
                         */
                    });
            }
        },
    };
})(jQuery, Drupal, drupalSettings);

function getJsonPoi(url_json) {
    let username = "16892599758";
    let password = "mas7Hsg5$q?a";
    let auth = btoa(`${username}:${password}`);
    fetch(url_json, {
        headers: {
            Authorization: `Basic ${auth}`,
        },
    })
        .then(function (response) {
            if (response.ok) {
                console.log("dati ricevuti");
                return response.json();
            }
            throw response;
        })
        .then(function (data) {
            console.log(data);

            const glJson = {};
            for (const obj of data) {
                glJson[obj.id] = obj;
            }
            console.log(glJson);
            sessionStorage.setItem("glJson", JSON.stringify(glJson));

            // Oggetto JSON vuoto per aggregare gli oggetti
            let jsonObject = {};

            // Itera sull'array e aggrega gli oggetti con lo stesso ID
            data.forEach((obj) => {
                const { id, ...rest } = obj;
                if (jsonObject[id]) {
                    // Se l'ID è già presente nell'oggetto aggregato, aggiungi l'oggetto corrente all'array
                    jsonObject[id].push(rest);
                } else {
                    // Altrimenti, crea un nuovo array con l'oggetto corrente e lo associa all'ID
                    jsonObject[id] = [rest];
                }
            });

            // Converti l'oggetto aggregato in una stringa JSON

            //jsonObject = JSON.stringify(jsonObject);
            console.log(jsonObject);
            console.log("elementi:" + Object.entries(jsonObject).length);
            sessionStorage.setItem("poidata_byid", JSON.stringify(jsonObject));
        })
        .catch(function (error) {
            console.warn(error);
        });
    console.log("ora qui");
}

function getSferiche(url_sferiche) {
    fetch(url_sferiche)
        .then((response) => response.json())
        .then((data) => {
            // Itera attraverso gli oggetti presenti in "sferiche"
            sessionStorage.setItem("sferiche", JSON.stringify(data.sferiche));
        })
        .catch((error) => {
            console.error("Errore durante il caricamento dei dati JSON:", error);
        });
}
