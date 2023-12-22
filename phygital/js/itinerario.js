(function ($, Drupal, drupalSettings) {
    let onlyOnce;
    Drupal.behaviors.itinerario = {
        attach: function (context, settings) {
            const dev_root = "https://dev.phygital.bbsitalia.com";
            const prod_root = "https://testnew.visitgenoa.it";
            const env_root = location.origin;
            if (!onlyOnce) {
                onlyOnce = true;
                $(document, context)
                    .once("itinerario")
                    .each(function () {
                        /*
                         * INIZIO CODICE JAVASCRIPT
                         */
                        //traduzioni
                        var current_language = drupalSettings.path.currentLanguage;

                        trans_municipality_itinerary = Drupal.t("Itinerari del Comune");
                        trans_user_itinerary = Drupal.t("Itinerari dell’Utente");

                        const trans_empty_path = {
                            it: "Percorso vuoto",
                            en: "Empty path",
                            fr: "Chemin vide",
                            es: "Ruta vacía",
                            ru: "Пустой путь",
                        };

                        const trans_mobile_unavailable = {
                            it: "Funzionalità attualmente non disponibile su dispositivi mobili",
                            en: "Functionality currently not available on mobile devices",
                            fr: "Fonctionnalité actuellement non disponible sur les appareils mobiles",
                            es: "Funcionalidad actualmente no disponible en dispositivos móviles",
                            ru: "Функционал в настоящее время не доступен на мобильных устройствах",
                        };

                        console.log("itinerario");

                        user_code = jQuery("body").attr('"data-user-code"');
                        user_id = jQuery("body").attr('"data-user-id"');
                        if (user_id === undefined || user_id == 0) user_code = 0;
                        console.log("codice utente: " + user_code);

                        //FORZATURA PER DEV.PHYGITAL
                        if (location.origin == dev_root) {
                            //user_code = "16892599758";
                            //user_id = 73;
                            user_code = "16832116725";
                            user_id = 21;
                        }

                        contacache = 1;
                        contaPOI = [];

                        urlbase = window.location.origin;

                        cache = true;
                        POIit = null;

                        it_poi = [];
                        it_punti = [];
                        it_opunti = [];
                        it_percorso = "";
                        init_tipi = [];
                        marker = [];

                        pagmodifica = "crea-itinerario";

                        ///////////////////////////

                        var markerspoi = [];

                        function mostraPOI(tipo, show) {
                            customIcon = {
                                iconUrl: urlbase + "/modules/custom/phygital/css/files/" + tipo + ".png",
                                iconSize: [30, 30],
                            };
                            myIcon = L.icon(customIcon);

                            customIconDisab = {
                                iconUrl: urlbase + "/modules/custom/phygital/css/files/999d.png",
                                iconSize: [30, 30],
                            };
                            myIconDisab = L.icon(customIconDisab);

                            listaPOI.forEach(function (poi) {
                                if (poi.lat != null && poi.id_tipologia != null && poi.id_tipologia == tipo) {
                                    if (!show) {
                                        marker[poi.id].remove();
                                    } else {
                                        myIconDaUsare = myIcon;
                                        if (poi.extra) {
                                            if (poi.extra.accessibile == "SI") {
                                                myIconDaUsare = myIconDisab;
                                            }
                                        }
                                        if (parseInt(poi.id) >= 998000) {
                                            if (poi.id_tipologia == "998") {
                                                titlepoi = "Bike";
                                            } else if (poi.id_tipologia == "999") {
                                                titlepoi = "Bagno";
                                            }
                                            if (myIconDaUsare == myIconDisab) {
                                                titlepoi += " (accessibilità disabili)";
                                            }
                                        } else {
                                            titlepoi = poi.title;
                                        }

                                        //console.log(poi.id);
                                        //console.log(poi.lat);
                                        //console.log(poi.lon);
                                        marker[poi.id] = L.marker([poi.lat, poi.lon], {
                                            idpoi: poi.id,
                                            icon: myIconDaUsare,
                                            title: titlepoi,
                                        }).addTo(map);

                                        markerspoi.push(marker);

                                        if (poi.id < 100000) {
                                            marker[poi.id].on("click", function () {
                                                contenuto = dettaglioPOI(this.options.idpoi);
                                                popup = "<h3>" + contenuto.nome + "</h3>";
                                                if (contenuto.immagine_di_copertina != "") {
                                                    popup += '<img class="img_copertina" src="' + contenuto.immagine_di_copertina + '">';
                                                }
                                                //popup += '<button class="aggiungi" onclick="aggiungi(' + this.options.idpoi + ',\'\');">Aggiungi all\'itinerario</button>';

                                                /*
                                                popup = "<h3>" + poi.extra.dati + "</h3>";
                                                if (poi.extra.accessibile!=undefined) popup += "Accessibile: <b>" + poi.extra.accessibile + "</b><br>";
                                                if (poi.extra.operativo!=undefined) popup += "Operativo: <b>" + poi.extra.operativo + "</b><br>";
                                                if (poi.extra.note!=undefined) popup += "Note: " + poi.extra.note + "<br>";
                                                */

                                                var popup = L.popup({ offset: [0, -20] })
                                                    .setLatLng(this.getLatLng())
                                                    .setContent(popup)
                                                    .openOn(map);

                                                // Imposta la nuova posizione del popup
                                                var newLatLng = L.latLng(this.getLatLng().lat, this.getLatLng().lng);
                                                popup.setLatLng(newLatLng);
                                            });
                                        }
                                    }
                                }
                            });
                        }

                        function dettaglioPOI(id) {
                            //url = "https://testnew.visitgenoa.it/jsonapi/post_poi_details"; // Replace with your API URL
                            url = env_root + "/fetch_api_call.php";
                            const nid = id;

                            POIit = null;

                            const requestBody = new FormData();
                            requestBody.append("nid", nid);
                            requestBody.append("endpoint", "post_poi_details");

                            dati = "";

                            if (id >= 999000 && id <= 999999) {
                                //bagni
                            } else if (id >= 999000 && id <= 999999) {
                                //bike
                            } else {
                                $.ajax({
                                    url: url,
                                    type: "POST",
                                    async: false,
                                    dataType: "json",
                                    data: requestBody,
                                    processData: false,
                                    contentType: false,
                                    /*
                                    beforeSend: function(xhr) {
                                        xhr.setRequestHeader("Authorization", "Basic " + btoa(username + ":" + password));
                                    },
                                    */
                                    success: function (data) {
                                        dati = data[0];
                                    },
                                    error: function (error) {
                                        // Request failed, handle the error here
                                        console.error("Error: Request failed.", error);
                                    },
                                });
                            }

                            return dati;
                        }

                        function convertCoordinates(x, y) {
                            // Definisci il sistema di coordinate di partenza (EPSG:3003) e di destinazione (EPSG:4326 - WGS84)
                            const fromProjection =
                                "+proj=tmerc +lat_0=0 +lon_0=9 +k=0.999600 +x_0=1500000 +y_0=0 +ellps=intl +towgs84=-104.1,-49.1,-9.9,0.971,-2.917,0.714,-11.68 +units=m +no_defs";
                            const toProjection = "+proj=longlat +ellps=WGS84 +datum=WGS84 +no_defs";

                            // Carica le definizioni dei sistemi di coordinate in proj4js
                            proj4.defs(fromProjection);
                            proj4.defs(toProjection);

                            // Esegui la conversione delle coordinate da EPSG:3003 a EPSG:4326
                            const lonLat = proj4(fromProjection, toProjection, [x, y]);

                            // Estrai le coordinate geografiche (latitudine e longitudine) risultanti
                            const longitude = lonLat[0];
                            const latitude = lonLat[1];

                            // Restituisci le coordinate geografiche come oggetto
                            return { latitude, longitude };
                        }

                        function listaPOI() {
                            //url = "https://testnew.visitgenoa.it/jsonapi/get_poi_list";
                            //url = "https://dev.phygital.bbsitalia.com/get_poi_list.html?" + contacache;
                            url = env_root + "/fetch_api_call.php?endpoint=get_poi_list";

                            $.ajax({
                                url: url,
                                type: "GET",
                                dataType: "json",
                                async: false,
                                /*
                                headers: {
                                    "Authorization": "Basic " + btoa(username + ':' + password)
                                },
                                */
                                success: function (data) {
                                    // bagni 999
                                    url =
                                        "https://mappe.comune.genova.it/geoserver/SITGEO/ows?service=WFS&version=1.0.0&request=GetFeature&typeName=$SITGEO:V_BAGNI&outputFormat=json";
                                    $.ajax({
                                        url: url,
                                        type: "GET",
                                        async: false,
                                        success: function (bagni) {
                                            bagni.features.forEach(function (bagno) {
                                                id = 999000 + parseInt(bagno.properties.ID, 10);
                                                lat = convertCoordinates(bagno.geometry.coordinates[0], bagno.geometry.coordinates[1]).latitude;
                                                lon = convertCoordinates(bagno.geometry.coordinates[0], bagno.geometry.coordinates[1]).longitude;
                                                data.push({
                                                    id: id,
                                                    lat: lat,
                                                    lon: lon,
                                                    id_tipologia: "999",
                                                    tipo: "poi",
                                                    extra: {
                                                        accessibile: bagno.properties.ACCESSIBILE,
                                                        dati: bagno.properties.DATI,
                                                        note: bagno.properties.NOTE,
                                                        operativo: bagno.properties.OPERATIVO,
                                                    },
                                                });
                                            });
                                        },
                                        error: function (error) {
                                            console.error("Error: Request failed.", error);
                                        },
                                    });

                                    // bike 998
                                    url =
                                        "https://mappe.comune.genova.it/geoserver/SITGEO/ows?service=WFS&version=1.0.0&request=GetFeature&typeName=$SITGEO:V_MOB_PARKS_BIKESHARING&outputFormat=json";
                                    $.ajax({
                                        url: url,
                                        type: "GET",
                                        async: false,
                                        success: function (bikes) {
                                            bikes.features.forEach(function (bike) {
                                                id = 998000 + parseInt(bike.properties.ID, 10);
                                                lat = convertCoordinates(bike.geometry.coordinates[0], bike.geometry.coordinates[1]).latitude;
                                                lon = convertCoordinates(bike.geometry.coordinates[0], bike.geometry.coordinates[1]).longitude;
                                                data.push({
                                                    id: id,
                                                    lat: lat,
                                                    lon: lon,
                                                    id_tipologia: "998",
                                                    tipo: "poi",
                                                    extra: { dati: bike.properties.NOME },
                                                });
                                            });
                                        },
                                        error: function (error) {
                                            console.error("Error: Request failed.", error);
                                        },
                                    });

                                    //console.log(contaPOI)

                                    localStorage.setItem("listaPOI", JSON.stringify(data));
                                },
                                error: function (error) {
                                    // Request failed, handle the error here
                                    console.error("Error: Request failed.", error);
                                },
                            });
                        }

                        function listaTipologiePOI() {
                            //url = "https://dev.phygital.bbsitalia.com/get_tipologie_poi.html?" + contacache;
                            url = env_root + "/fetch_api_call.php?endpoint=get_tipologie_poi";
                            $.ajax({
                                url: url,
                                type: "GET",
                                dataType: "json",
                                async: false,
                                success: function (data) {
                                    //for (z=0; z<data.length; z++) {
                                    //contaPOI[data[z]['id']] = 0;
                                    //console.log(data[z]['id_tipologia']);
                                    //}
                                    data.push({ id: "999", nome: "Bagni" });
                                    //contaPOI[999] = 0;
                                    data.push({ id: "998", nome: "Bike" });
                                    //contaPOI[998] = 0;
                                    localStorage.setItem("listaTipiPOI", JSON.stringify(data));
                                },
                                error: function (error) {
                                    console.error("Error: Request failed.", error);
                                },
                            });
                        }

                        if (localStorage.getItem("listaTipiPOI") == null || !cache) {
                            listaTipologiePOI();
                        }
                        listaTipiPOI = localStorage.getItem("listaTipiPOI");
                        listaTipiPOI = JSON.parse(listaTipiPOI);
                        //console.log(listaTipiPOI);

                        for (z = 0; z < listaTipiPOI.length; z++) {
                            contaPOI[listaTipiPOI[z]["id"]] = 0;
                        }

                        if (localStorage.getItem("listaPOI") == null || !cache) {
                            listaPOI();
                        }

                        listaPOI = localStorage.getItem("listaPOI");
                        listaPOI = JSON.parse(listaPOI);
                        //console.log(listaPOI);

                        for (z = 0; z < listaPOI.length; z++) {
                            if (listaPOI[z]["lat"] != null) {
                                contaPOI[listaPOI[z]["id_tipologia"]]++;
                            }
                            //console.log(data[z]['id_tipologia']);
                        }

                        ////////////////////////////

                        function ditidel(id) {
                            const url = env_root + "/fetch_api_call.php";
                            const nid = id;
                            const endpoint = "post_itinerario_delete";

                            const requestBody = new FormData();
                            requestBody.append("nid", nid);
                            requestBody.append("endpoint", endpoint);
                            // Add more parameters if needed

                            fetch(url, {
                                method: "POST",
                                body: requestBody,
                            })
                                .then((response) => response.text())
                                .then((data) => {
                                    // Successful response, print the HTML content
                                    console.log(data);
                                    // document.querySelectorAll("#test").
                                    // document.write(response);
                                    // document.write(data);
                                    // document.write("ciao");
                                })
                                .catch((error) => {
                                    // Request failed, handle the error here
                                    console.error("Error: Request failed.", error);
                                });
                        }
                        // Call the function to make the POST request

                        function diti(id, nome, cont) {
                            if (!confirm("Vuoi eliminare l'itinerario ''" + nome + "''?")) {
                                return false;
                            }

                            if (typeof routeLayer !== "undefined") {
                                routeLayer.remove(map);
                            }
                            cont.remove();

                            map.eachLayer(function (layer) {
                                if (layer instanceof L.Popup) {
                                    map.closePopup(layer);
                                }
                            });

                            ditidel(id);
                        }

                        function dpoi(id) {
                            //url = "https://testnew.visitgenoa.it/jsonapi/post_poi_details"; // Replace with your API URL
                            url = env_root + "/fetch_api_call.php";
                            const nid = id;

                            const requestBody = new FormData();
                            requestBody.append("nid", nid);
                            requestBody.append("endpoint", "post_poi_details");

                            dati = "";

                            if (id >= 999000 && id <= 999999) {
                                //bagni
                            } else if (id >= 999000 && id <= 999999) {
                                //bike
                            } else {
                                $.ajax({
                                    url: url,
                                    type: "POST",
                                    async: false,
                                    dataType: "json",
                                    data: requestBody,
                                    processData: false,
                                    contentType: false,
                                    /*
                                    beforeSend: function(xhr) {
                                        xhr.setRequestHeader("Authorization", "Basic " + btoa(username + ":" + password));
                                    },
                                    */
                                    success: function (data) {
                                        dati = data[0];
                                    },
                                    error: function (error) {
                                        // Request failed, handle the error here
                                        console.error("Error: Request failed.", error);
                                    },
                                });
                            }

                            return dati;
                        }

                        function itin(id) {
                            //url = "https://testnew.visitgenoa.it/jsonapi/post_itinerario_details"; // Replace with your API URL
                            url = env_root + "/fetch_api_call.php";
                            const nid = id;

                            const requestBody = new FormData();
                            requestBody.append("nid", nid);
                            requestBody.append("endpoint", "post_itinerario_details");
                            // Add more parameters if needed

                            dati = "";
                            $.ajax({
                                url: url,
                                type: "POST",
                                async: false,
                                dataType: "json",
                                data: requestBody,
                                processData: false,
                                contentType: false,
                                /*
                                beforeSend: function(xhr) {
                                    xhr.setRequestHeader("Authorization", "Basic " + btoa(username + ":" + password));
                                        },
                                */
                                success: function (data) {
                                    // Successful response, print the HTML content
                                    //console.log(data);
                                    //console.log(JSON.stringify(data,null,2))

                                    if (JSON.parse(data[0]["sferiche"]) !== null) {
                                        POIit = JSON.parse(data[0]["sferiche"]).lista_poi;
                                    }
                                },
                                error: function (error) {
                                    // Request failed, handle the error here
                                    console.error("Error: Request failed.", error);
                                },
                            });

                            return dati;
                        }

                        function listait() {
                            //url = "https://testnew.visitgenoa.it/jsonapi/post_itinerari_list"; // Replace with your API URL
                            url = env_root + "/fetch_api_call.php";
                            const uid = user_code;

                            const requestBody = new FormData();
                            requestBody.append("uid", uid);
                            requestBody.append("endpoint", "post_itinerari_list");

                            $.ajax({
                                url: url,
                                type: "POST",
                                async: false,
                                dataType: "json",
                                data: requestBody,
                                processData: false,
                                contentType: false,
                                /*
                                beforeSend: function(xhr) {
                                    xhr.setRequestHeader("Authorization", "Basic " + btoa(username + ":" + password));
                                },
                                */
                                success: function (data) {
                                    // Successful response, print the data
                                    console.log(data);
                                    localStorage.setItem("listaItinerari", JSON.stringify(data));

                                    /*	
                                    data.forEach(function(poi) {
                                        console.log(poi.id)
                                        })					
                                    */
                                },
                                error: function (error) {
                                    // Request failed, handle the error here
                                    console.error("Error: Request failed.", error);
                                },
                            });
                        }

                        listait();

                        if (localStorage.getItem("listaItinerari") == null) {
                            listait();
                        }
                        listaiti = localStorage.getItem("listaItinerari");
                        listaiti = JSON.parse(listaiti);

                        tipoiti = 1;
                        $(".itinerari-cont").append('<div class="tipoiti citdisp">' + trans_municipality_itinerary + ":</div>");
                        listaiti.forEach(function (i) {
                            if (tipoiti != i.predefinito) {
                                $(".itinerari-cont").append('<div class="tipoiti citdisp">' + trans_user_itinerary + ":</div>");
                            }
                            tipoiti = i.predefinito;

                            if (i.predefinito == 1) {
                                bdel = "";
                                classbdel = "";
                            } else {
                                bdel =
                                    '<button data-it="' +
                                    i.id +
                                    '" title="Modifica l\'itinerario" class="moditi">&#9998;</button><button data-it="' +
                                    i.id +
                                    '" title="Elimina l\'itinerario" class="deleteiti">&#10005;</button>';
                                classbdel = " delmod";
                            }
                            $(".itinerari-cont").append(
                                '<div class="citdisp"><button data-it="' +
                                    i.id +
                                    '" class="itdisp' +
                                    classbdel +
                                    '">' +
                                    i.title +
                                    "</button>" +
                                    bdel +
                                    '</div><div id="t' +
                                    i.id +
                                    '" class="tappe"></div>'
                            );
                            i.lista_poi.forEach(function (jj) {
                                //console.log(listaPOI.length)

                                if (jj.id_poi.indexOf(",") < 0) {
                                    listaPOI.forEach(function (poi) {
                                        if (poi.id == jj.id_poi) {
                                            $(".itinerari-cont #t" + i.id).append('<div class="openpoi" data-poi="' + it_poi[i] + '">' + poi.title + "</div>");

                                            return;
                                        }
                                    });
                                }
                            });
                        });

                        $(".moditi").click(function () {
                            top.location.href = pagmodifica + "?id=" + $(this).data("it");
                        });

                        $(".deleteiti").click(function () {
                            diti($(this).data("it"), $(this).parent().find(".itdisp").text(), $(this).parent());
                        });

                        if (localStorage.getItem("listaPOI") == null || !cache) {
                            listaPOI();
                        }
                        listaPOI = localStorage.getItem("listaPOI");
                        listaPOI = JSON.parse(listaPOI);

                        var map = L.map("map").setView([44.414169, 8.928349], 16);
                        L.tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
                            maxZoom: 19,
                            //attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
                        }).addTo(map);

                        listaTipiPOI.forEach(function (tipo) {
                            if (contaPOI[tipo.id] > 0) {
                                // 133 ne ha solo una e nulla
                                // 35 non ci sono le tipologie nella lista
                                $(".itinerari2").append(
                                    '<button z-index="999" id="tipoPOI' +
                                        tipo.id +
                                        '" class="butttipo attiv"><b>&#10004;</b><img src="' +
                                        urlbase +
                                        "/modules/custom/phygital/css/files/" +
                                        tipo.id +
                                        '.png"><!--span id="spanPOI' +
                                        tipo.id +
                                        '">x</span-->' +
                                        tipo.nome +
                                        "</button>"
                                );
                                init_tipi[tipo.id] = true;
                                mostraPOI(tipo.id, init_tipi[tipo.id]);
                            }
                        });

                        //click

                        testo2 = "Scegli il tuo itinerario";
                        $("#iniziamo").click(function () {
                            $(this).hide();
                            $(".titoloner").hide();
                            $(".itinerari").show();
                            $(".fumetto").html(testo2);
                        });

                        $(".itdisp").click(function () {
                            //$(".cloader").fadeIn(100)
                            //$(".tappe").html("");
                            //$("#avvia").hide();
                            $(".itdisp").not($(this)).removeClass("att");
                            $(".tappe")
                                .not("#t" + $(this).data("it"))
                                .hide();
                            $(".tappe#t" + $(this).data("it")).show();
                            $(this).addClass("att");

                            if (typeof routeLayer !== "undefined") {
                                routeLayer.remove(map);
                            }

                            map.eachLayer(function (layer) {
                                if (layer instanceof L.Popup) {
                                    map.closePopup(layer);
                                }
                            });

                            iti = itin($(this).data("it"));

                            it_percorso = "";
                            it_punti = [];
                            it_poi = [];

                            if (POIit == undefined) {
                                if (trans_empty_path.hasOwnProperty(current_language)) {
                                    alert(trans_empty_path[current_language]);
                                } else {
                                    alert(trans_empty_path["it"]);
                                }
                                return false;
                            }

                            for (i = 0; i < POIit.length; i++) {
                                const idPoi = POIit[i].id_poi;

                                it_poi.push(idPoi);
                                //console.log(idPoi);

                                const obj = listaPOI.find(function (item) {
                                    return item.id === idPoi;
                                });

                                // Crea un array contenente le coordinate e aggiungilo ad it_punti
                                if (typeof obj !== "undefined") {
                                    coordinate = [obj.lat, obj.lon];
                                    objlon = obj.lon;
                                    objlat = obj.lat;
                                } else {
                                    coordinate = [idPoi.split(",")[1], idPoi.split(",")[0]];
                                    objlon = idPoi.split(",")[1];
                                    objlat = idPoi.split(",")[0];
                                }
                                //alert(coordinate)
                                it_punti.push(coordinate);

                                if (i == 0) {
                                    it_percorso += "&start=" + objlon + "," + objlat;
                                } else if (i == POIit.length - 1) {
                                    it_percorso += "&end=" + objlon + "," + objlat;
                                } else {
                                    it_percorso += "&intermediate=" + objlon + "," + objlat;
                                }
                            }

                            routeLayer = L.geoJSON(null, {
                                style: {
                                    color: "red",
                                    weight: 3,
                                    opacity: 0.7,
                                    lineCap: "round",
                                    lineJoin: "round",
                                    //dashArray: '10, 5, 2, 5'
                                },
                                className: "pippoz",
                            });

                            var markers = [];

                            var redmarker = new L.Icon({
                                iconUrl: urlbase + "/modules/custom/phygital/files/marker-icon.png",
                                shadowUrl: urlbase + "/modules/custom/phygital/files/marker-shadow.png",
                                iconSize: [25, 33],
                                iconAnchor: [12.5, 30],
                                shadowSize: [41, 41],
                                shadowAnchor: [12.5, 37],
                                className: "mi",
                            });

                            for (ii = 0; ii < it_punti.length - 1; ii++) {
                                //var api_key = ...
                                //var url = 'https://api.openrouteservice.org/v2/directions/foot-walking?api_key=' + api_key + "&start=" + it_punti[ii][1] + "," + it_punti[ii][0] + "&end=" + it_punti[ii+1][1] + "," + it_punti[ii+1][0];
                                var url =
                                    env_root +
                                    "/ors_proxy.php?" +
                                    "start=" +
                                    it_punti[ii][1] +
                                    "," +
                                    it_punti[ii][0] +
                                    "&end=" +
                                    it_punti[ii + 1][1] +
                                    "," +
                                    it_punti[ii + 1][0];
                                console.log(url);
                                fetch(url)
                                    .then((response) => response.json())
                                    .then((data) => {
                                        // var coordinates = data.features[0].geometry.coordinates;
                                        // // Crea un layer per i punti intermedi

                                        // for (var i = 0; i < coordinates.length; i++) {
                                        // var marker = L.marker([coordinates[i][1], coordinates[i][0]]);
                                        // //routeLayer.addLayer(marker);
                                        // }

                                        // var marker = L.marker(it_punti[ii], {

                                        // idpoi: it_poi[ii]
                                        // });
                                        // markers.push(marker);
                                        // // routeLayer.addLayer(marker);

                                        // marker.on('click', function() {
                                        // //alert(this.options.idpoi)
                                        // contenuto=dpoi(this.options.idpoi);
                                        // popup="<h3>"+contenuto.nome+"</h3>"
                                        // if(contenuto.immagine_di_copertina!="") {popup+='<img class="img_copertina" src="' + contenuto.immagine_di_copertina + '">';}

                                        // var popup = L.popup({offset: [0, -20]})
                                        // .setLatLng(this.getLatLng())
                                        // .setContent(popup)
                                        // .openOn(map);

                                        // // Imposta la nuova posizione del popup
                                        // var newLatLng = L.latLng(this.getLatLng().lat, this.getLatLng().lng);
                                        // popup.setLatLng(newLatLng);
                                        // });

                                        // Aggiungi i dati della rotta al layer
                                        routeLayer.addData(data);
                                        //map.fitBounds(routeLayer.getBounds());
                                    });
                            }
                            fetch(url)
                                .then((response) => response.json())
                                .then((data) => {
                                    /*

                                    var coordinates = data.features[0].geometry.coordinates;
                                    // Crea un layer per i punti intermedi

                                    for (var i = 0; i < coordinates.length; i++) {
                                        var marker = L.marker([coordinates[i][1], coordinates[i][0]], { icon: redmarker, zIndexOffset: 10000 });
                                        //routeLayer.addLayer(marker);
                                    }

                                    */

                                    /*
                                    for(i=0;i<it_punti.length;i++) {

                                        var marker = L.marker(it_punti[i], { 
                                            icon: redmarker,
                                            zIndexOffset: 10000,
                                            idpoi: it_poi[i] 
                                        });
                                        //routeLayer.addLayer(marker); //aggiunge i marker personalizzati

                                        //if($.isNumeric(it_poi[i])) { $(".itdisp.att").parent().next().append('<div class="openpoi" data-poi="'+it_poi[i]+'">'+dettaglioPOI(it_poi[i]).nome+'</div>') }

                                        if(i==(it_punti.length-1)) {$(".cloader").fadeOut(100)}

                                        marker.on('click', function() {
                                            //alert(this.options.idpoi)
                                            popup="";

                                            if (this.options.idpoi.indexOf(",")<0){
                                                contenuto=dpoi(this.options.idpoi);
                                                popup="<h3>"+contenuto.nome+"</h3>"
                                                if(contenuto.immagine_di_copertina!="") {popup+='<img class="img_copertina" src="' + contenuto.immagine_di_copertina + '">';}
                                            }
                                            else{

                                                popup="Lat.: " + this.getLatLng().lat.toFixed(7) + " - Lon.: " + this.getLatLng().lng.toFixed(7) 
                                            }

                                            var popup = L.popup({offset: [0, -20]}) 
                                            .setLatLng(this.getLatLng())
                                            .setContent(popup)
                                            .openOn(map);

                                            // Imposta la nuova posizione del popup
                                            var newLatLng = L.latLng(this.getLatLng().lat, this.getLatLng().lng);
                                            popup.setLatLng(newLatLng);
                                        });

                                    }
                                    */

                                    // Aggiungi i dati della rotta al layer
                                    // routeLayer.addData(data);
                                    map.fitBounds(routeLayer.getBounds());

                                    if (POIit !== undefined) {
                                        window.itinerario = { nome: "demo", id: String(user_code), lista_poi: [] };
                                        for (const item of it_poi) {
                                            itinerario["lista_poi"].push({ id_poi: String(item) });
                                        }
                                        sessionStorage.setItem("dati_app_360", JSON.stringify(itinerario));
                                        const jsonDataItinerario = JSON.stringify(itinerario);
                                        const expirationDays = 1;
                                        const expirationDate = new Date();
                                        expirationDate.setDate(expirationDate.getDate() + expirationDays);
                                        document.cookie = `dati_app_360=${encodeURIComponent(
                                            jsonDataItinerario
                                        )}; expires=${expirationDate.toUTCString()}; path=/`;
                                        $("#avvia").show();
                                    }
                                });

                            routeLayer.addTo(map);

                            POIit = null;
                        });

                        window.iniziaTour360 = function (user_language) {
                            if ($(window).width() < 768) {
                                if (trans_mobile_unavailable.hasOwnProperty(current_language)) {
                                    alert(trans_mobile_unavailable[current_language]);
                                } else {
                                    alert(trans_mobile_unavailable["it"]);
                                }
                            } else {
                                var linkUrl = "/web360/index.html?lingua=" + user_language;
                                window.open(linkUrl, "_blank");
                            }
                        };

                        setTimeout('jQuery(".cloader").fadeOut(100)', 300);

                        /*
                         * FINE CODICE JAVASCRIPT
                         */
                    });
            }
        },
    };
})(jQuery, Drupal, drupalSettings);
