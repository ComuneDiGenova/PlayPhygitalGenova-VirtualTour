/**/



/**/
(function($,Drupal,drupalSettings){
  let onlyOnce;
  Drupal.behaviors.viewtipologie={
    attach:function(context,settings){
      if (!onlyOnce) {
        onlyOnce = true;
        $(document, context).once('viewtipologie').each(function () {
          // $('#mappa-parcheggi-tv').insertBefore('#header');
          //$('#overlay_attesa').insertBefore('#header');
          // $('#header').remove();
          //$('#main-wrapper').remove();
          //$('footer').remove();
          /*
          * INIZIO CODICE JAVASCRIPT
          */
          console.log('leaflet map');

          console.log('leaflet map fine');
          /*
          * FINE CODICE JAVASCRIPT
          */
        });
      }
    }
  }
}(jQuery,Drupal,drupalSettings));
