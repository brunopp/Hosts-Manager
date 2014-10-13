
chrome.runtime.onMessage.addListener(function (data) {
	$('body').append('<div style="position:fixed;left:1%;top:2%;width:150px;background:white;padding:1em;color: #9F6000;background-color: #FEEFB3;border: solid 1px #9F6000;box-shadow: 1px 1px 5px rgba(0,0,0,0.3);z-index: 99999999;">' + data.IP + '</div>');
});
