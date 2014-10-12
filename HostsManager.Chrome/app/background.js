
chrome.tabs.onUpdated.addListener(function (tabId, changeInfo, tab) {
	if (changeInfo.status !== 'loading')
		return;

	var url = tab.url;

	var patt = /^(https?:\/\/)?([^\/]+)/g;
	var match = patt.exec(url);

	if (!match[1])
		return;

	$.get('http://hosts/api/Mapping/Get?domain=' + match[2] + '&onlyActive=true').done(function (data) {
		if (data !== null) {
			chrome.tabs.executeScript(tabId, { file: "jquery-2.1.1.min.js" }, function () {
				chrome.tabs.executeScript(tabId, { file: "content-script.js" }, function () {
					chrome.tabs.sendMessage(tabId, data, function () {
						
					});
				});
			});

			chrome.pageAction.show(tabId);
		} else {
			chrome.pageAction.hide(tabId);
		}
	});
});
