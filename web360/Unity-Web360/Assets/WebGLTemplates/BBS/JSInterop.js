
//FULLLSCREEN

window.addEventListener('load', resizePage);
window.addEventListener('resize', resizePage);

function resizePage()
{  
    canvas.style.width = (window.innerWidth) + 'px';
    canvas.style.height = (window.innerHeight) + 'px';
}


function SendItinerario(itinerario, unityInstance) {
    unityInstance.SendMessage("[JSInterop]", "Itinerario", itinerario)
}


function SaveItinerario(itinere){
    sessionStorage.setItem("dati_app_360", itinere);
}

function ReadItinerario(){
    let itinere = sessionStorage.getItem("dati_app_360");
    return itinere;
}

function ClearData(){
    sessionStorage.clear();
}

function FakeItinerario() {
    let itinere=`{"nome":"demo","id":"16892599758","lingua":"IT","rgb":"#000000","lista_poi":[{"id_poi":"1555"},{"id_poi":"1543"},{"id_poi":"1622"}]}`;
    //let itinere = `{"nome":"demo","id":"16892599758","lingua":"IT","lista_poi":[{"id_poi":"1555"},{"id_poi":"1543"},{"id_poi":"1622"},{"id_poi":"null"},{"id_poi":"8.934023811525375,44.40773809151876"}]}`;
    return itinere;
}

function FakeCookie() {
    let itinere = FakeItinerario();
    let cookie = encodeURIComponent(itinere);
    return decodeURIComponent(cookie);
}

function GetCookie(name) {
   // const cookieString = decodeURIComponent(document.cookie);
   // const cookies = cookieString.split('; ');
    if (document.cookie === null) return null;
    
    const cookies = document.cookie.split('; ');
  
    for (const cookie of cookies) {
      const [cookieName, cookieValue] = cookie.split('=');
      if (cookieName === name) {
          //return cookieValue;
          return decodeURIComponent(cookieValue);
      }
    }
    
    return null;
}