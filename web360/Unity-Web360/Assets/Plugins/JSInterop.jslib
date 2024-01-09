mergeInto(LibraryManager.library, {

  CloseWindow: function(){
    console.log("Close windows");
    if (confirm("Close Window?")) {
        window.open(location.href, "_self", "");
        window.close();
    }
  },

  GetURLFromPage: function () {
    var returnStr = window.top.location.href;
    var bufferSize = lengthBytesUTF8(returnStr) + 1
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  
  SyncFiles : function(){
    FS.syncfs(false,function (err) {
      if (err) console.log("syncfs error: " + err);
      SendMessage('JSInterop', 'EndSync');
    });
  },

  ReadFiles : function(){
    FS.syncfs(true,function (err) {
      if (err) console.log("syncfs error: " + err);
      SendMessage('JSInterop', 'EndSync');
    });
  }

});
