(function ($) {
	let Nitems = 4;//10;
	let iPage = 0;	
	let prepair = $('#prepair');

	let filter = {
		code: '',
		value: '',
		iPage: iPage,
		N: Nitems
	};

	let item = {
		code: 0,
		value: ''
	};	

	let Items = [];

	$('#setPrepair').click(function () {
		let xcode = $('#xcode').val();
		let xvalue = $('#xvalue').val();

		if (xcode != '' && xvalue != '') {
			item = {
				id: null,
				code: parseInt(xcode),
				value: xvalue
			};
			Items.push(item);

			$('#prepair').append('<p class="list-group-item">code:' + xcode + ' <span class="label label-success">value:' + xvalue + '</span></p>');
			$('#xcode').empty();
			$('#xvalue').empty();
		}
	});

	$('#sendAll').click(function () {
		let list = Items;		
		$.ajax({
			type: 'POST',
			url: '/Home/SaveItems',
			contentType: 'application/json; charset=utf-8',
			dataType: 'json',
			data: 
				JSON.stringify(list),
			
			success: function (result) {
				prepair.empty();
				$('#xcode').empty();
				$('#xvalue').empty();
				$('#prepair').empty();
			}
		});
	});

	
	$('#prevPage').bind("click", function (e) {
		e.preventDefault();
	});

	$('#nextPage').bind("click", function (e) {
		e.preventDefault();
	});


	$('#getItems').click(function () {
		getItems();
	});

	$('#prevPage').click(function () {
		iPage--;
		if (iPage < 0) iPage = 0;
		getItems();
	});

	$('#nextPage').click(function () {
		if (haveRows) {
			iPage++;
			getItems();			
		}		
	});

	let haveRows = false;

	function getItems() {
		
		filter.code = $('#xcode').val();
		filter.value = $('#xvalue').val();
		filter.N = Nitems;
		filter.iPage = iPage;
		
		let request = filter;

		$.ajax({
			type: 'POST',
			url: '/Home/GetData',
			contentType: 'application/json; charset=utf-8',
			dataType: 'json',
			data:
				JSON.stringify(request),

			success: function (result) {

				if (result) {
					let row = '';

				
					if (haveRows && result.length == 0) {
						haveRows = false;						
						iPage--;
					}
					else if (result.length > 0) {
						haveRows = true;						
					}

					if (haveRows)
						$('#resultItems').empty();

					var i = 0;
					while (i < result.length) {
						row += '<p>code:' + result[i].code + ' <span>value:' + result[i].value + '</span></p>';
						i++;
					}
					$('#resultItems').append(row);
				}
				else {
					if (!haveRows) {						
						iPage--;
						if (iPage < 0) iPage = 0;
					}
				}
				
				$('#xcode').empty();
				$('#xvalue').empty();
			}
		});
	}

}) (jQuery);
