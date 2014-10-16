
(function ($) {
	$.fn.drags = function (opt) {

		opt = $.extend({ handle: "", cursor: "move" }, opt);

		if (opt.handle === "") {
			var $el = this;
		} else {
			var $el = this.find(opt.handle);
		}

		return $el.css('cursor', opt.cursor).on("mousedown", function (e) {
			if (opt.handle === "") {
				var $drag = $(this).addClass('draggable');
			} else {
				var $drag = $(this).addClass('active-handle').parent().addClass('draggable');
			}

			var z_idx = $drag.css('z-index'),
				drg_h = $drag.outerHeight(),
				drg_w = $drag.outerWidth(),
				pos_y = $drag.offset().top + drg_h - e.pageY,
				pos_x = $drag.offset().left + drg_w - e.pageX;

			// reset right and bottom
			$drag.offset({
				top: $drag.offset().top,
				left: $drag.offset().left
			});
			$drag.css('right', '');
			$drag.css('bottom', '');

			$drag.css('z-index', 1000).parents().on("mousemove", function (e) {
				if (e.which === 1) {
					$('.draggable').offset({
						top: e.pageY + pos_y - drg_h,
						left: e.pageX + pos_x - drg_w
					}).on("mouseup", function () {
						$(this).removeClass('draggable').css('z-index', z_idx);
					});
				}

			});
			e.preventDefault(); // disable selection
		}).on("mouseup", function () {
			if (opt.handle === "") {
				$(this).removeClass('draggable');
			} else {
				$(this).removeClass('active-handle').parent().removeClass('draggable');
			}

			//
			// calculate position
			//
			var w = $(window);
			var b = $(this);

			// left/top positions to right/bottom positions
			var topToBottom = w.height() - b.position().top - b.outerHeight(true);
			var leftToRight = w.width() - b.position().left - b.outerWidth(true);

			var middleXPoint = b.position().left + b.outerWidth() / 2;
			var middleYPoint = b.position().top + b.outerHeight() / 2;

			var isOnRightSide = middleXPoint > w.width() / 2;
			var isOnBottomSide = middleYPoint > w.height() / 2;

			var mapping = {
				Id: b.data('id')
			};

			if (isOnRightSide) {
				b.css('left', '');
				b.css('right', leftToRight + 'px');
				mapping['XRight'] = Math.round(leftToRight);
			} else {
				mapping['XLeft'] = Math.round(b.offset().left);
			}
			if (isOnBottomSide) {
				b.css('top', '');
				b.css('bottom', topToBottom + 'px');
				mapping['YBottom'] = Math.round(topToBottom);
			} else {
				mapping['YTop'] = Math.round(b.offset().top);
			}

			$.ajax({
				url: 'http://hosts/api/Mapping/UpdateCoords/' + mapping.Id,
				type: 'PUT',
				data: mapping
			});
		});
	}
})(jQuery);


chrome.runtime.onMessage.addListener(function (data) {
	if (data && $('#hostsManagerInfoBox[data-id=' + data.Id + ']').length === 0) {
		var div =
			$('<div id="hostsManagerInfoBox" data-id="'+data.Id+'" style="position:fixed;width:150px;background:white;padding:1em;color: #9F6000;background-color: #FEEFB3;border: solid 1px #9F6000;box-shadow: 1px 1px 5px rgba(0,0,0,0.3);z-index: 99999999;padding: 8px;font-size: 12px;">'
			+ data.IP +
			'</div>');

		if (data.XLeft) {
			div.offset({
				left: data.XLeft
			});
		}
		if (data.YTop) {
			div.offset({
				top: data.YTop
			});
		}
		if (data.XRight) {
			div.css('right', data.XRight + 'px');
		}
		if (data.YBottom) {
			div.css('bottom', data.YBottom + 'px');
		}

		div.drags();
		$('body').append(div);
	}
});
