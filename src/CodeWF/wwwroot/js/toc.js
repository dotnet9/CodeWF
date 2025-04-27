window.getTOC = function getTOC(id) {
  let hlevel = "h1, h2, h3, h4";
  var titles = $("#" + id)
    .find(hlevel)
    .map(function () {
      return { type: this.tagName.toLowerCase(), id: $(this).prop("id"), title: $(this).text() };
    })
    .get();

  const currentUrl = window.location.href.split("#")[0];
  titles.forEach(function (item) {
    const link = `<a class="item-${item.type}" href="${currentUrl}#${item.id}" title="${item.title}">${item.title}</a>`;
    const wrappedLink = `<div>${link}</div>`;
    $("#toc").append(wrappedLink);
  });

  $(window).scroll(function (){
      $("#toc").toggleClass('toc-show', $(this).scrollTop() > 400);
  });
  tocUp(200);
};

function tocUp(val) {
  // 平滑滚动到对应位置
  $("body").on("click", "#toc a", function (event) {
    event.preventDefault(); // 阻止默认链接行为
    const targetId = $(this).attr("href").split("#")[1]; 
    const targetElement = $(`#${targetId}`);

    if (targetElement.length) {
      const offset = targetElement.offset().top - val;
      $("html, body").animate({ scrollTop: offset }, 1200); // 平滑滚动
    }
  });
}
