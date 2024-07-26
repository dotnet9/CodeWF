window.MasaOfficialWebsite = {}

window.MasaOfficialWebsite.scrollToNext = () => {
  const innerHeight = window.innerHeight || document.body.clientHeight
  const offsetY = document.body.scrollTop || document.documentElement.scrollTop;
  const n = Math.ceil((offsetY / innerHeight) || 1)
  animationScrollTo(innerHeight * n)
}

window.MasaOfficialWebsite.scrollToPrev = () => {
  const innerHeight = window.innerHeight || document.body.clientHeight
  const offsetY = document.body.scrollTop || document.documentElement.scrollTop;
  const n = Math.ceil((offsetY / innerHeight) || 1)
  if (n - 1 > -1) {
    animationScrollTo(innerHeight * (n - 1))
  }
}

window.MasaOfficialWebsite.scrollToNextForTouch = () => {
  const innerHeight = window.innerHeight || document.body.clientHeight
  const offsetY = document.body.scrollTop || document.documentElement.scrollTop;

  if (offsetY < innerHeight) {
    const n = Math.ceil((offsetY / innerHeight) || 1)
    animationScrollTo(innerHeight * n)
  }
}

window.MasaOfficialWebsite.scrollToPrevForTouch = () => {
  const innerHeight = window.innerHeight || document.body.clientHeight
  const offsetY = document.body.scrollTop || document.documentElement.scrollTop;

  if (offsetY < innerHeight) {
    const n = Math.ceil((offsetY / innerHeight) || 1)
    if (n - 1 > -1) {
      animationScrollTo(innerHeight * (n - 1))
    }
  }
}

window.MasaOfficialWebsite.scrollTo = (selector) => {
  const dom = document.querySelector(selector)
  if (dom) {
    animationScrollTo(dom.offsetTop)
  }
}

function animationScrollTo(top) {
  const offsetY = document.body.scrollTop || document.documentElement.scrollTop;

  let timer

  const c = offsetY - top
  const startTime = +new Date();
  const duration = 500;

  cancelAnimationFrame(timer)

  timer = requestAnimationFrame(function f() {
    const time = duration - Math.max(0, startTime - (+new Date()) + duration);
    document.body.scrollTop = document.documentElement.scrollTop = time * (-c) / duration + offsetY;
    timer = requestAnimationFrame(f)

    if (time === duration) {
      cancelAnimationFrame(timer)
    }
  })
}