(function ($, Drupal, drupalSettings) {
    let onlyOnce;
    Drupal.behaviors.creaitinerario = {
        attach: function (context, settings) {
            const dev_root = "https://dev.phygital.bbsitalia.com";
            const prod_root = "https://testnew.visitgenoa.it";
            const env_root = location.origin;
            if (!onlyOnce) {
                onlyOnce = true;
                $(document, context)
                    .once("creaitinerario")
                    .each(function () {
                        /*
                         * INIZIO CODICE JAVASCRIPT
                         */
                        //traduzioni
                        var current_language = drupalSettings.path.currentLanguage;

                        trans_add_to_itinerary = Drupal.t("Aggiungi all’itinerario");
                        trans_your_itinerary = Drupal.t("Il tuo itinerario");
                        trans_cancel = Drupal.t("Cancella");
                        trans_from = Drupal.t("da:");
                        trans_to = Drupal.t("a:");
                        trans_remove_from_itinerary = Drupal.t("Rimuovi dall’itinerario");
                        trans_edit_path = Drupal.t("Modifica il tuo percorso");
                        trans_create_path = Drupal.t("Inizia a costruire il tuo percorso");
                        trans_remove_from_map = Drupal.t("Rimuovi dalla mappa");

                        const trans_insert_itinerary_name = {
                            it: "Inserisci un nome per il tuo itinerario",
                            en: "Enter a name for your itinerary",
                            fr: "Donnez un nom à votre itinéraire",
                            es: "Introduzca un nombre para su itinerario",
                            ru: "Введите название маршрута",
                        };

                        const trans_available_poi = {
                            it: "Questo punto è presente nel tuo itinerario.\nPrima di rimuoverlo eliminalo dal tuo itinerario",
                            en: "This point is present in your itinerary.\nBefore removing it, delete it from your itinerary",
                            fr: "Ce point est présent dans votre itinéraire.\nAvant de le supprimer, supprimez-le de votre itinéraire",
                            es: "Este punto está presente en su itinerario.\nAntes de eliminarlo, bórrelo de su itinerario",
                            ru: "Этот пункт присутствует в маршруте.\nПрежде чем удалить его, удалите его из маршрута",
                        };

                        const trans_empty_path = {
                            it: "Percorso vuoto",
                            en: "Empty path",
                            fr: "Chemin vide",
                            es: "Ruta vacía",
                            ru: "Пустой путь",
                        };

                        console.log("benvenuto creaitinerario.js");

                        user_code = jQuery("body").attr('"data-user-code"');
                        user_id = jQuery("body").attr('"data-user-id"');
                        predefinito = 0;

                        if (user_id === undefined || user_id == 0) user_code = 0;

                        //FORZATURA PER DEV.PHYGITAL
                        if (location.origin == dev_root) {
                            //user_code = "16892599758";
                            //user_id = 73;
                            user_code = "16832116725";
                            user_id = 21;
                        }

                        ///////////////////////////////////////
                        // SOVRASCRITTURA VALORI PER UTENTE COMUNALE, COMMENTARE PER UTENTE FINALE
                        //   predefinito = 1;
                        ///////////////////////////////////////

                        console.log("codice utente: " + user_code);
                        console.log("utente comunale: " + (predefinito ? "SI" : "NO"));

                        contacache = 1;
                        contaPOI = [];

                        localStorage.clear(); //???

                        urlbase = window.location.origin;
                        urlversione = "/sites/default/files/sferiche/versionesferiche.json";
                        urlsferiche = "/sites/default/files/sferiche/sferiche.json";

                        iditi = "";
                        urlParams = new URLSearchParams(window.location.search);
                        if (urlParams.get("id") != null) {
                            iditi = urlParams.get("id").trim();
                        }

                        nomeiti = "";

                        if (iditi != "") {
                            $("#iniziaperc").text(trans_edit_path);
                        } else {
                            $("#iniziaperc").text(trans_create_path);
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

                        window.delitin = function () {
                            it_poi = [];
                            it_punti = [];
                            it_opunti = [];
                            $(".itinerari3").html("");
                            $("#fumetto2").hide();
                            routeLayer.remove(map);
                            markerGroup.eachLayer(function (layer) {
                                // rimuovi il marker dal layerGroup
                                markerGroup.removeLayer(layer);
                            });
                        };

                        window.delnpoi = function (n) {
                            $(n).parent().remove();
                            it_poi.pop();
                            it_opunti.pop();
                            //alert(it_opunti.length)
                            if (it_opunti.length < 2) {
                                $("#fumetto2").hide();
                            }
                            if (it_opunti.length > 1 && 1 == 2) {
                                $("[data-npoi=" + (it_opunti.length - 1) + "]").append('<small onclick="delnpoi(this)" class="delpoi">x</small>');
                            }

                            if (routeLayer != null) {
                                routeLayer.remove(map);
                            }
                            $("#prosegui").trigger("click");
                        };

                        window.aggiungi = function (id, coo, address) {
                            //alert(id)

                            //NASCONDO LA SCRITTA E LA LINEA SOTTOSTANTE "Seleziona sulla mappa le tappe del tuo itinerario" QUANDO L'UTENTE AGGIUNGE IL PRIMO PUNTO ALL'ITINERARIO
                            $(".fumetto").hide();

                            if ($(".itinerari3 .p" + id).length == 0) {
                                if (it_opunti.length == 0) {
                                    if (nomeiti != "") {
                                        nn = ': <b class="nomeiti">' + nomeiti + "</b>";
                                    } else {
                                        nn = "";
                                    }
                                    $(".itinerari3").append(
                                        "<b>" +
                                            trans_your_itinerary +
                                            "</b>" +
                                            nn +
                                            '<small onclick="delitin()" class="delitin">' +
                                            trans_cancel +
                                            "</small><br>"
                                    );
                                }

                                if (coo == "") {
                                    c = dettaglioPOI(id);
                                    $(".itinerari3").append(
                                        '<div data-npoi="' +
                                            it_opunti.length +
                                            '" class="punti p' +
                                            id +
                                            '">' +
                                            (it_opunti.length == 0 ? trans_from + " " : trans_to + " ") +
                                            c.nome +
                                            "</div>"
                                    );
                                } else {
                                    idd = id;
                                    idd = idd.replace(",", "").replace(".", "").replace(".", "");
                                    if (iditi != "" && address.indexOf("Lat.:") > -1) {
                                        address = "";
                                    }
                                    $(".itinerari3").append(
                                        '<div data-npoi="' +
                                            it_opunti.length +
                                            '" class="pers punti p' +
                                            idd +
                                            '">' +
                                            (it_opunti.length == 0 ? trans_from + " " : trans_to + " ") +
                                            address +
                                            "</div>"
                                    );
                                }

                                if (coo == "") {
                                    var elemento = [];
                                    elemento.push(L.latLng(c.latitudine, c.longitudine));
                                    it_punti.push(elemento);
                                    it_poi.push(id);
                                    it_opunti.push(c.longitudine + "," + c.latitudine);
                                } else {
                                    it_poi.push(coo);
                                    it_opunti.push(coo);
                                }
                                //alert(it_opunti.length)
                                $(".delpoi").remove();
                                if (it_opunti.length > 1 && 1 == 2) {
                                    $("[data-npoi=" + (it_opunti.length - 1) + "]").append('<small onclick="delnpoi(this)" class="delpoi">x</small>');
                                }

                                //marker[id].click();
                                $("div.leaflet-popup-pane").html("");

                                //chiude i popup
                                map.eachLayer(function (layer) {
                                    if (layer instanceof L.Popup) {
                                        map.closePopup(layer);
                                    }
                                });

                                //creapercorso()
                            }

                            if (routeLayer != null) {
                                routeLayer.remove(map);
                            }
                            $(".itinerari3").scrollTop($(".itinerari3").height()); //////??
                            $("#prosegui").trigger("click");
                        };
                        window.iniziaTour360 = function (user_language) {
                            var linkUrl = "/web360/index.html?lingua=" + user_language;
                            window.open(linkUrl, "_blank");
                        };
                        /*
                        function creapercorso(){
                            //aggiunta Davide
                            window.itinerario={nome:"demo",id:123,lista_poi:[]};
                            for (const item of it_poi) {
                                itinerario["lista_poi"].push({"id_poi": String(item)});
                            }
                            sessionStorage.setItem("dati_app_360", JSON.stringify(itinerario)); 				
                            const jsonDataItinerario = JSON.stringify(itinerario);
                            const expirationDays = 1;
                            const expirationDate = new Date();
                            expirationDate.setDate(expirationDate.getDate() + expirationDays);
                            document.cookie = `dati_app_360=${encodeURIComponent(jsonDataItinerario)}; expires=${expirationDate.toUTCString()}; path=/`;
                            //fine aggiunta Davide
                            //FAUSTINI
                            it_percorso="";
                            for(i=0;i<it_opunti.length;i++) {
                                if(i==0) {
                                    it_percorso+="&start="+it_opunti[i]
                                }
                                else if(i==(it_opunti.length-1)) {
                                    it_percorso+="&end="+it_opunti[i]
                                }
                                else {
                                    it_percorso+="&intermediate="+it_opunti[i]
                                }
                            }
                            // FINE FAUSTINI
                            routeLayer = L.geoJSON(null, {
                                style: {
                                color: 'red'
                                },
                                className: 'pippoz'
                            });
            
                            // Chiama l'API di OpenRouteService per il calcolo della rotta
                            //https://api.openrouteservice.org/v2/directions/driving-car?api_key=...&start=-0.1,51.5&end=-0.18,51.55&intermediate=-0.14,51.52&intermediate=-0.15,51.53
                            var api_key = ...
                            var url = 'https://api.openrouteservice.org/v2/directions/foot-walking?api_key=' + api_key + it_percorso;
                            console.log(url)
                            fetch(url)
                                .then(response => response.json())
                                .then(data => {
                                var coordinates = data.features[0].geometry.coordinates;
                                // Crea un layer per i punti intermedi
                                
                                for (var i = 0; i < coordinates.length; i++) {
                                    var marker = L.marker([coordinates[i][1], coordinates[i][0]]);
                                    //routeLayer.addLayer(marker);
                                }
                                // Aggiungi i dati della rotta al layer
                                routeLayer.addData(data);
                                });	
                            routeLayer.addTo(map);				
                        }
                        */
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
                                            zIndexOffset: 1000,
                                        }).addTo(map);

                                        if (poi.id < 100000) {
                                            marker[poi.id].on("click", function () {
                                                contenuto = dettaglioPOI(this.options.idpoi);
                                                popup = "<h3>" + contenuto.nome + "</h3>";
                                                if (contenuto.immagine_di_copertina != "") {
                                                    popup += '<img class="img_copertina" src="' + contenuto.immagine_di_copertina + '">';
                                                }
                                                if ($(".itinerari3 .p" + this.options.idpoi).length == 0) {
                                                    popup +=
                                                        '<button class="aggiungi" onclick="aggiungi(' +
                                                        this.options.idpoi +
                                                        ",'');\">" +
                                                        trans_add_to_itinerary +
                                                        "</button>";
                                                } else {
                                                    popup +=
                                                        '<div class="crimuovi"><button class="rimuovi" onclick="rimuoviMarker(\'\',\'' +
                                                        this.getLatLng().lng +
                                                        "," +
                                                        this.getLatLng().lat +
                                                        "','p" +
                                                        poi.id +
                                                        "');\">" +
                                                        trans_remove_from_itinerary +
                                                        " &nbsp;&#10005;</button></div>";
                                                }

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

                        cache = true;

                        it_poi = [];
                        it_punti = [];
                        it_opunti = [];
                        it_percorso = "";
                        init_tipi = [];
                        marker = [];

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

                        //console.log(contaPOI);

                        routeLayer = null;

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
                                        '.png" alt="tipoPOI' +
                                        tipo.id +
                                        '"><!--span id="spanPOI' +
                                        tipo.id +
                                        '">x</span-->' +
                                        tipo.nome +
                                        "</button>"
                                );
                                init_tipi[tipo.id] = true;
                                mostraPOI(tipo.id, init_tipi[tipo.id]);
                            }
                        });

                        testo2 = "Scegli il tuo itinerario";

                        $("[id^='tipoPOI']").click(function () {
                            id = $(this).prop("id");
                            id = id.replace("tipoPOI", "");
                            mostraPOI(id, !init_tipi[id]);
                            init_tipi[id] = !init_tipi[id];
                            if (init_tipi[id]) {
                                $(this).addClass("attiv");
                            } else {
                                $(this).removeClass("attiv");
                            }
                            //$('#spanPOI' + id).html(init_tipi[id] ? 'x' : '&nbsp;');
                            //$('#tipoPOI' + id).css('background', init_tipi[id] ? 'var(--rosso)' : 'var(--grey)');
                        });
                        /*
                        $("#prosegui").click(function(){
                            creapercorso()
                        });
                        */

                        var redmarker = new L.Icon({
                            iconUrl: urlbase + "/modules/custom/phygital/files/marker-icon.png",
                            shadowUrl: urlbase + "/modules/custom/phygital/files/marker-shadow.png",
                            iconSize: [25, 33],
                            iconAnchor: [12.5, 30],
                            shadowSize: [41, 41],
                            shadowAnchor: [12.5, 37],
                            popupAnchor: [0, -28],
                            className: "mi",
                        });

                        $("#prosegui").click(function () {
                            if (it_opunti.length < 2) {
                                //alert("Seleziona almeno due punti")
                                return false;
                            }

                            if (routeLayer != null) {
                                routeLayer.remove(map);
                            }

                            //METTERE USERID L'USERID = 0, PERCHÈ NON DEVE AGGIUNGERE PUNTI O PREFERITI
                            const user_code_itinerario = predefinito ? "0" : user_code;
                            window.itinerario = { nome: "demo", id: String(user_code_itinerario), lista_poi: [] };

                            for (const item of it_poi) {
                                itinerario["lista_poi"].push({ id_poi: String(item) });
                            }
                            //sessionStorage.setItem("dati_app_360", JSON.stringify(itinerario));
                            const jsonDataItinerario = JSON.stringify(itinerario);
                            console.log("itinerario: " + jsonDataItinerario);
                            const expirationDays = 1;
                            const expirationDate = new Date();
                            expirationDate.setDate(expirationDate.getDate() + expirationDays);
                            document.cookie = `dati_app_360=${encodeURIComponent(jsonDataItinerario)}; expires=${expirationDate.toUTCString()}; path=/`;

                            //console.log(it_opunti);
                            for (ii = 0; ii < it_opunti.length - 1; ii++) {
                                // Chiama l'API di OpenRouteService per il calcolo della rotta
                                //https://api.openrouteservice.org/v2/directions/driving-car?api_key=...&start=-0.1,51.5&end=-0.18,51.55&intermediate=-0.14,51.52&intermediate=-0.15,51.53
                                //var api_key = ...
                                //var url = 'https://api.openrouteservice.org/v2/directions/foot-walking?api_key=' + api_key + "&start=" + it_opunti[ii] + "&end=" + it_opunti[ii+1]
                                var url = env_root + "/ors_proxy.php?" + "start=" + it_opunti[ii] + "&end=" + it_opunti[ii + 1];

                                routeLayer = L.geoJSON(null, { style: { color: "red" }, className: "pippoz" });

                                //console.log(url)
                                fetch(url)
                                    .then((response) => response.json())
                                    .then((data) => {
                                        //console.log(data);
                                        var coordinates = data.features[0].geometry.coordinates;
                                        // Crea un layer per i punti intermedi

                                        for (var i = 0; i < coordinates.length; i++) {
                                            var marker = L.marker([coordinates[i][1], coordinates[i][0]]);
                                            //routeLayer.addLayer(marker);
                                        }

                                        // Aggiungi i dati della rotta al layer
                                        routeLayer.addData(data);
                                    });
                                //routeLayer.addTo(map);
                                //routeLayer.remove(map);
                                routeLayer.addTo(map);
                            }

                            $("#fumetto2").show();

                            if (iditi == "") {
                                $("#nomePercorso").val("").focus();
                            }
                        });

                        $(".itdisp").click(function () {
                            $(".itdisp").not($(this)).removeClass("att");
                            $(this).addClass("att");
                            aitinerari[$(this).data("it")].spliceWaypoints(0, it_punti[$(this).data("it")].length);
                            itinere($(this).data("it"), it_punti[$(this).data("it")], "S");
                        });

                        $("#salva").click(function () {
                            if ($("#nomePercorso").val().trim() == "") {
                                if (trans_insert_itinerary_name.hasOwnProperty(current_language)) {
                                    alert(trans_insert_itinerary_name[current_language]);
                                } else {
                                    alert(trans_insert_itinerary_name["it"]);
                                }
                                $("#nomePercorso").focus();
                                return false;
                            }

                            const url = env_root + "/fetch_api_call.php";
                            const user_code_itinerario = predefinito ? "0" : user_code;
                            const user_id = user_code;
                            let sferiche = ""; //`{"nome":"demo","id":0,"lista_poi":[{"id_poi":"1603"},{"id_poi":"1536"}]}`;
                            const titolo = $("#nomePercorso").val();
                            const endpoint = "post_itinerario_add";
                            if (iditi != "") {
                                nid = iditi;
                            }

                            //CREO L'ITINERARIO CON user_id = '' e ID=0 così diventa predefinito. Deve essere salvato dall'api come non pubblicato. Appena il comune lo pubblicherà l'api di reperimento degli itinerari lo includerà nei risultati
                            sferiche = '{"nome": "' + $("#nomePercorso").val() + '", "id": "' + user_code_itinerario + '", "lista_poi": [';

                            for (z = 0; z < it_poi.length; z++) {
                                if (z > 0) {
                                    sferiche += ",";
                                }
                                sferiche += '{"id_poi": "' + it_poi[z] + '"}';
                            }
                            sferiche += "]}";

                            console.log(sferiche);

                            const requestBody = new FormData();
                            requestBody.append("uid", user_id);
                            requestBody.append("user_id", user_id);
                            requestBody.append("sferiche", sferiche);
                            requestBody.append("titolo", titolo);
                            requestBody.append("predefinito", predefinito);
                            requestBody.append("endpoint", endpoint);
                            if (iditi != "") {
                                requestBody.append("nid", nid);
                            }
                            // Add more parameters if needed
                            console.log(requestBody);

                            fetch(url, {
                                method: "POST",
                                body: requestBody,
                            })
                                .then((response) => response.text())
                                .then((data) => {
                                    // Successful response, print the HTML content
                                    console.log(data);
                                    $("#fumetto2").hide();
                                    $("#avvia").show();
                                })
                                .catch((error) => {
                                    // Request failed, handle the error here
                                    console.error("Error: Request failed.", error);
                                });

                            // url = "https://dev.phygital.bbsitalia.com/fetch_api_call.php";

                            // const requestBody = new FormData();
                            // requestBody.append('endpoint', 'post_itinerario_add');
                            // requestBody.append('user_id', 1);
                            // requestBody.append('Titolo', $('#nomePercorso').val());
                            // sferiche = '{"nome": "' + $('#nomePercorso').val() + '", "Ã¯d": 123, "lista_poi": [';
                            // for (z=0; z<it_poi.length; z++) {
                            // if (z>0) {
                            // sferiche += ',';
                            // }
                            // sferiche += '{"id_poi": ' + it_poi[z] + '}';
                            // }
                            // sferiche += ']}';
                            // requestBody.append('sferiche', sferiche);
                            // $.ajax({
                            // url: url,
                            // type: "POST",
                            // data: requestBody,
                            // //dataType: "json",
                            // async: false,
                            // processData: false,
                            // contentType: false,
                            // success: function(data) {
                            // console.log(data);
                            // $('#fumetto2').hide();
                            // $('#avvia').show();
                            // },
                            // error: function(error) {
                            // console.error("Error: Request failed.", error);
                            // }
                            // });
                        });
                        console.log("saluto creaitinerario");

                        //aggiunta punti

                        window.rimuoviMarker = function (id, coo, classe) {
                            console.log(it_opunti);
                            if (jQuery.inArray(coo, it_opunti) == -1) {
                                if (id != "") {
                                    markerGroup.eachLayer(function (layer) {
                                        if (layer._id == id) {
                                            // rimuovi il marker dal layerGroup
                                            markerGroup.removeLayer(layer);
                                        }
                                    });
                                }

                                return false;
                            }
                            it_opunti.splice(it_opunti.indexOf(coo), 1);
                            it_poi.splice(it_poi.indexOf(coo), 1);

                            if (jQuery.inArray(coo, it_opunti) !== -1) {
                                if (trans_available_poi.hasOwnProperty(current_language)) {
                                    alert(trans_available_poi[current_language]);
                                } else {
                                    alert(trans_available_poi["it"]);
                                }
                                return false;
                            }
                            if (id != "") {
                                markerGroup.eachLayer(function (layer) {
                                    if (layer._id == id) {
                                        // rimuovi il marker dal layerGroup
                                        markerGroup.removeLayer(layer);
                                    }
                                });
                            } else {
                                map.eachLayer(function (layer) {
                                    if (layer instanceof L.Popup) {
                                        map.closePopup(layer);
                                    }
                                });
                            }

                            $("." + classe).remove();
                            if (routeLayer != null) {
                                routeLayer.remove(map);
                            }
                            if (it_opunti.length < 1) {
                                $(".itinerari3").html("");
                            }
                            if (it_opunti.length < 2) {
                                $("#fumetto2").hide();
                            }
                            $("#prosegui").trigger("click");
                        };

                        var markerGroup = L.layerGroup().addTo(map);
                        //markerGroup.setZIndex(99999999999);

                        map.doubleClickZoom.disable();
                        //map.on('dblclick', function(event) {
                        // Aggiunge un gestore per l'evento click sulla mappa
                        //map.on('contextmenu', function(event) {

                        map.on("dblclick", function (event) {
                            var idm = Math.random().toString(36).substr(2, 9);

                            // Ottiene le coordinate del punto in cui è stato effettuato il click
                            var lat = event.latlng.lat;
                            var lng = event.latlng.lng;

                            // Crea un nuovo marker nella posizione del click
                            var marker = L.marker([lat, lng], {
                                icon: redmarker,
                                id: idm,
                                zIndexOffset: 1000,
                            }).addTo(markerGroup);
                            marker._id = idm;

                            // Aggiunge un popup con le coordinate e l'indirizzo al marker

                            /*
                            popup="Lat.: " + lat.toFixed(7) + " - Lon.: " + lng.toFixed(7) 
                            address=popup
                            popup += '<br><br><button class="aggiungi" onclick="aggiungi(\''+idm+'\',\'' + lng+','+lat + '\',\'' + address + '\');">Aggiungi all\'itinerario</button>';
                            popup += '<div class="crimuovi"><button class="rimuovi" onclick="rimuoviMarker(\''+idm+'\',\'' + lng+','+lat + '\');">Rimuovi dalla mappa &nbsp;&#10005;</button></div>';
                            marker.bindPopup(popup).openPopup();
                            */

                            L.Control.Geocoder.nominatim().reverse(event.latlng, map.options.crs.scale(map.getZoom()), function (results) {
                                var address = results[0].name;
                                address = address.replace(", Genoa", "");
                                address = address.replace(", Liguria", "");
                                address = address.replace(", Italia", "");
                                //address=address.split(",")[0].trim()+', '+address.split(",")[1].trim()+', '+address.split(",")[2].trim()+', '+address.split(",")[3].trim()+', '+address.split(",")[6].trim()
                                //popup="Latitudine: " + lat + "<br>Longitudine: " + lng + "<br>Indirizzo: " + address

                                //popup="Latitudine: " + lat + " - Longitudine: " + lng
                                //address=popup

                                popup = address;
                                //popup += '<br><br>';

                                popup +=
                                    '<button class="aggiungi aggiungi' +
                                    idm +
                                    '" onclick="aggiungi(\'' +
                                    idm +
                                    "','" +
                                    lng +
                                    "," +
                                    lat +
                                    "','" +
                                    address.replace("'", "\\'") +
                                    "');\">" +
                                    trans_add_to_itinerary +
                                    "</button>";
                                popup +=
                                    '<div class="crimuovi crimuovi' +
                                    idm +
                                    '"><button class="rimuovi" onclick="rimuoviMarker(\'' +
                                    idm +
                                    "','" +
                                    lng +
                                    "," +
                                    lat +
                                    "','p" +
                                    idm +
                                    "');\">" +
                                    trans_remove_from_itinerary +
                                    " &nbsp;&#10005;</button></div>";

                                marker.on("popupopen", function () {
                                    if ($(".itinerari3 .p" + idm).length == 0) {
                                        $(".crimuovi" + idm + " button").text(trans_remove_from_map + " \u00A0\u2715");
                                    } else {
                                        $(".aggiungi" + idm).hide();
                                        $(".crimuovi" + idm + " button").text(trans_remove_from_itinerary + " \u00A0\u2715");
                                    }
                                });

                                marker.bindPopup(popup).openPopup();
                                marker.getElement().setAttribute("title", address);

                                // markerGroup.getLayer(marker._leaflet_id).bindPopup("pippo");
                            });
                        });

                        //modifica itinerario/////////////////////////////////////////////

                        var indirizzi = [];

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
                                    nomeiti = data[0]["nome"];
                                    $("#nomePercorso").val(data[0]["nome"].trim());
                                    if (data[0] == undefined) {
                                        if (trans_empty_path.hasOwnProperty(current_language)) {
                                            alert(trans_empty_path[current_language]);
                                        } else {
                                            alert(trans_empty_path["it"]);
                                        }
                                        return false;
                                    }
                                    console.log(JSON.stringify(data, null, 2));
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

                        if (iditi != "") {
                            itin(iditi);

                            moditi = [];
                            for (i = 0; i < POIit.length; i++) {
                                moditi.push(POIit[i].id_poi);
                            }

                            //alert(moditi)

                            if (POIit == undefined) {
                                if (trans_empty_path.hasOwnProperty(current_language)) {
                                    alert(trans_empty_path[current_language]);
                                } else {
                                    alert(trans_empty_path["it"]);
                                }
                                return false;
                            }

                            for (i = 0; i < moditi.length; i++) {
                                const idPoi = moditi[i];

                                const obj = listaPOI.find(function (item) {
                                    return item.id === idPoi;
                                });

                                if (typeof obj !== "undefined") {
                                    coordinate = [obj.lon, obj.lat];
                                    objlon = obj.lon;
                                    objlat = obj.lat;
                                    coo = "";
                                    nome = dettaglioPOI(idPoi).nome;
                                } else {
                                    coordinate = [idPoi.split(",")[0], idPoi.split(",")[1]];
                                    objlon = idPoi.split(",")[0];
                                    objlat = idPoi.split(",")[1];
                                    coo = objlon + "," + objlat;
                                    nome = "Lat.: " + parseFloat(objlat).toFixed(7) + " - Lon.: " + parseFloat(objlon).toFixed(7);

                                    var lat = parseFloat(objlat);
                                    var lng = parseFloat(objlon);

                                    var url = "https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=" + lat + "&lon=" + lng;
                                    var address = "";

                                    $.ajax({
                                        url: url,
                                        async: false,
                                        success: function (data) {
                                            address = data.display_name;

                                            address = address.replace(", Genoa", "");
                                            address = address.replace(", Liguria", "");
                                            address = address.replace(", Italia", "");

                                            indirizzi.push(address);
                                        },
                                    });

                                    idm = idPoi.split(",")[0] + idPoi.split(",")[1];
                                    idm = idm.replace(".", "").replace(".", "");
                                    var marker1 = L.marker([objlat, objlon], {
                                        icon: redmarker,
                                        id: idm,
                                        title: address,
                                        zIndexOffset: 1000,
                                    }).addTo(markerGroup);
                                    marker1._id = idm;

                                    popup = "Lat.: " + parseFloat(objlat).toFixed(7) + " - Lon.: " + parseFloat(objlon).toFixed(7);
                                    address = popup;
                                    //popup += '<br><br><button class="aggiungi" onclick="aggiungi(\''+idm+'\',\'' + objlon+','+objlat + '\',\'' + address + '\');">Aggiungi all\'itinerario</button>';
                                    popup = "";
                                    popup +=
                                        '<div class="crimuovi"><button class="rimuovi" onclick="rimuoviMarker(\'' +
                                        idm +
                                        "','" +
                                        objlon +
                                        "," +
                                        objlat +
                                        "','p" +
                                        idm +
                                        "');\">" +
                                        trans_remove_from_itinerary +
                                        " 		&nbsp;&#10005;</button></div>";
                                    //marker.bindPopup(popup).openPopup();
                                    marker1.bindPopup(popup);
                                }

                                //it_poi.push(idPoi);
                                //it_opunti.push(coordinate);

                                aggiungi(idPoi, coo, nome);
                            }

                            console.log(it_opunti);

                            //$("#prosegui").trigger("click");

                            setTimeout(function () {
                                for (var i = 0; i < indirizzi.length; i++) {
                                    var indiri = indirizzi[i];
                                    markerGroup.getLayers()[i].bindPopup(indiri + markerGroup.getLayers()[i].getPopup().getContent());
                                    $(".pers")
                                        .eq(i)
                                        .text($(".pers").eq(i).text() + indiri);
                                }
                                $(".itinerari3").scrollTop(0);
                            }, 1000);

                            setTimeout(function () {
                                map.fitBounds(routeLayer.getBounds());
                            }, 2000);
                        }

                        //////////////////////////////////////////

                        //versione sferiche	e sferiche
                        $.ajax({
                            url: urlversione,
                            async: false,
                            dataType: "json",
                            contentType: "application/json; charset=utf-8",
                            success: function (data) {
                                versione = data.versione;

                                if (localStorage.getItem("versione") != versione) {
                                    localStorage.setItem("versione", versione);

                                    $.ajax({
                                        url: urlsferiche,
                                        async: false,
                                        dataType: "json",
                                        contentType: "application/json; charset=utf-8",
                                        success: function (data) {
                                            console.log(data);
                                            // Rimuovere le chiavi indesiderate da ciascun oggetto "sferica"
                                            data.sferiche.forEach((item) => {
                                                for (const key in item) {
                                                    if (!["X", "Y", "Filename"].includes(key)) {
                                                        delete item[key];
                                                    }
                                                }
                                            });
                                            // Convertire il JSON filtrato in una stringa
                                            const filteredJsonString = JSON.stringify(data);
                                            //localStorage.setItem('datasferiche', JSON.stringify(data));
                                            console.log(filteredJsonString);
                                            localStorage.setItem("datasferiche", filteredJsonString);
                                        },
                                        error: function (xhr, textStatus, errorThrown) {
                                            console.log("Error:", errorThrown);
                                        },
                                    });
                                }
                            },
                            error: function (xhr, textStatus, errorThrown) {
                                console.log("Error:", errorThrown);
                            },
                        });

                        datasferiche = localStorage.getItem("datasferiche");
                        datasferiche = JSON.parse(datasferiche);

                        //var markers = L.markerClusterGroup({ chunkedLoading: true, maxClusterRadius: 80, disableClusteringAtZoom: 19 });
                        var markers = L.markerClusterGroup({
                            chunkedLoading: true,
                            maxClusterRadius: 80,
                            spiderfyOnMaxZoom: false,
                            polygonOptions: {
                                fillColor: "#0d91fd",
                                color: "#0d91fd",
                                weight: 1.5,
                                opacity: 1,
                                fillOpacity: 0.2,
                            },
                        });

                        var iconsfere = new L.Icon({
                            iconUrl: "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z/C/HgAGgwJjWgAAAABJRU5ErkJggg==",
                            iconSize: [10, 10],
                            iconAnchor: [0, 0],
                            className: "pallino",
                        });

                        datasferiche.sferiche.forEach(function (poi) {
                            var marker = L.marker(L.latLng(poi.Y, poi.X), {
                                icon: iconsfere,
                                alt: "pallino",
                            });
                            markers.addLayer(marker);
                        });

                        map.addLayer(markers);

                        $(".selpoi").click(function () {
                            $(".itinerari2").toggleClass("aperto");
                            $(this).toggleClass("aperto");
                            if ($(".itinerari2").hasClass("aperto")) {
                                $(".itinerari2").slideDown(200);
                            } else {
                                $(".itinerari2").slideUp(200);
                            }
                        });

                        setTimeout('jQuery(".leaflet-marker-icon.marker-cluster").attr("title","Sferiche");jQuery(".cloader").fadeOut(100)', 2000);

                        /*			
                        var greenIcon = new L.Icon({
                            iconUrl: 'trasp.png',
                            iconSize: [20, 20],
                            iconAnchor: [10, 10],
                            className: 'pallino'
                        });	
                                    
                        datasferiche.sferiche.forEach(function(poi) {
                        if(poi.Id<1000)
                            {
                                var marker = L.marker([poi.Y, poi.X], {
                                icon: greenIcon
                                }).addTo(map);
                                
                            marker.on('click', function(e) {
                                //alert("Coordinate: " + e.latlng.lat + ", " + e.latlng.lng);
                                coordinateArray.push([e.latlng.lat, e.latlng.lng]);
                                console.log(coordinateArray)
                            });
                            }
                        });
                        
                        */

                        /*
                         * FINE CODICE JAVASCRIPT
                         */
                    });
            }
        },
    };
})(jQuery, Drupal, drupalSettings);

jQuery(document).ready(function () {
    console.log("document ready");
    console.log(jQuery().jquery);
});
