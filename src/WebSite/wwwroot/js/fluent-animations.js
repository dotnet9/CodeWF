/**
 * FluentAnimations - 为页面元素添加顺序动画效果
 * 使用方法：
 * 1. 在元素上添加 fluent-animated 类和动画类型（如 fluent-slide-up）
 * 2. 可选：添加延迟类（如 fluent-delay-100）和持续时间类（如 fluent-duration-fast）
 * 3. 页面加载时会自动初始化动画
 */
class FluentAnimations {
  constructor(options = {}) {
    this.options = {
      rootMargin: '0px',
      threshold: 0.1,
      animateOnce: true,
      animateWhenVisible: true,
      defaultDelay: 100,
      defaultDuration: 'normal',
      ...options
    };
    
    this.animatedElements = [];
    this.observer = null;
    this.initialized = false;
  }

  /**
   * 初始化动画系统
   */
  init() {
    if (this.initialized) return;
    
    // 创建交叉观察器，用于检测元素是否进入视口
    if ('IntersectionObserver' in window) {
      this.observer = new IntersectionObserver(this._onIntersection.bind(this), {
        rootMargin: this.options.rootMargin,
        threshold: this.options.threshold
      });
      
      // 查找所有带有动画类的元素
      const elements = document.querySelectorAll('.fluent-animated');
      elements.forEach(element => {
        this.animatedElements.push(element);
        this.observer.observe(element);
      });
    } else {
      // 如果浏览器不支持 IntersectionObserver，则直接显示所有元素
      document.querySelectorAll('.fluent-animated').forEach(element => {
        element.classList.add('animated');
      });
    }
    
    // 如果不需要等待元素可见，则立即开始顺序动画
    if (!this.options.animateWhenVisible) {
      this._animateSequentially();
    }
    
    this.initialized = true;
  }

  /**
   * 处理元素进入视口的事件
   * @param {IntersectionObserverEntry[]} entries 
   */
  _onIntersection(entries) {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        // 如果元素进入视口，添加动画类
        if (this.options.animateWhenVisible) {
          this._animateElement(entry.target);
        }
        
        // 如果只需要动画一次，则停止观察该元素
        if (this.options.animateOnce) {
          this.observer.unobserve(entry.target);
        }
      } else if (!this.options.animateOnce && !this.options.animateWhenVisible) {
        // 如果元素离开视口且不是只动画一次，则移除动画类
        entry.target.classList.remove('animated');
      }
    });
  }

  /**
   * 为指定元素添加动画类
   * @param {HTMLElement} element 
   */
  _animateElement(element) {
    element.classList.add('animated');
  }

  /**
   * 顺序动画所有元素
   */
  _animateSequentially() {
    let delay = 0;
    
    this.animatedElements.forEach(element => {
      // 检查元素是否已有延迟类
      const hasDelayClass = Array.from(element.classList).some(cls => cls.startsWith('fluent-delay-'));
      
      if (!hasDelayClass) {
        // 如果没有延迟类，则添加计算的延迟
        element.style.transitionDelay = `${delay}ms`;
        delay += this.options.defaultDelay;
      }
      
      // 检查元素是否已有持续时间类
      const hasDurationClass = Array.from(element.classList).some(cls => cls.startsWith('fluent-duration-'));
      
      if (!hasDurationClass) {
        // 如果没有持续时间类，则添加默认持续时间
        element.classList.add(`fluent-duration-${this.options.defaultDuration}`);
      }
      
      // 添加动画类
      setTimeout(() => {
        this._animateElement(element);
      }, 10);
    });
  }

  /**
   * 重置所有动画
   */
  reset() {
    this.animatedElements.forEach(element => {
      element.classList.remove('animated');
      
      if (this.observer) {
        this.observer.observe(element);
      }
    });
  }

  /**
   * 为指定选择器的元素添加动画
   * @param {string} selector CSS选择器
   * @param {string} animationType 动画类型
   * @param {Object} options 选项
   */
  animate(selector, animationType, options = {}) {
    const elements = document.querySelectorAll(selector);
    const delay = options.delay || 0;
    const duration = options.duration || this.options.defaultDuration;
    const stagger = options.stagger || 100;
    
    elements.forEach((element, index) => {
      // 添加动画类
      element.classList.add('fluent-animated', animationType);
      
      // 设置延迟和持续时间
      element.style.transitionDelay = `${delay + (index * stagger)}ms`;
      element.classList.add(`fluent-duration-${duration}`);
      
      // 如果需要立即动画，则添加动画类
      if (options.immediate) {
        setTimeout(() => {
          this._animateElement(element);
        }, 10);
      } else if (this.observer) {
        // 否则添加到观察器
        this.animatedElements.push(element);
        this.observer.observe(element);
      }
    });
  }
}

// 页面加载完成后初始化动画
document.addEventListener('DOMContentLoaded', () => {
  // 创建全局动画实例
  window.fluentAnimations = new FluentAnimations({
    animateWhenVisible: true,
    defaultDelay: 100,
    defaultDuration: 'normal'
  });
  
  // 初始化动画
  window.fluentAnimations.init();
  
  // 为特定元素添加自定义动画
  const mediaBoxes = document.querySelectorAll('.fluent-media-box');
  Array.from(mediaBoxes).forEach((box, index) => {
    box.classList.add('fluent-animated', 'fluent-slide-up-fade');
    box.style.transitionDelay = `${index * 100}ms`;
  });
  
  // 为首页标题添加特殊动画
  const heroTitle = document.querySelector('.jumbotron h2');
  if (heroTitle) {
    heroTitle.classList.add('fluent-animated', 'fluent-slide-right', 'fluent-duration-slow');
  }
  
  // 为首页副标题添加特殊动画
  const heroSubtitles = document.querySelectorAll('.jumbotron h5');
  heroSubtitles.forEach((subtitle, index) => {
    subtitle.classList.add('fluent-animated', 'fluent-slide-right', 'fluent-duration-normal');
    subtitle.style.transitionDelay = `${200 + (index * 150)}ms`;
  });
  
  // 为首页按钮添加特殊动画
  const heroButtons = document.querySelectorAll('.jumbotron .btn');
  heroButtons.forEach((button, index) => {
    button.classList.add('fluent-animated', 'fluent-slide-up', 'fluent-duration-normal');
    button.style.transitionDelay = `${500 + (index * 100)}ms`;
  });
  
  // 为首页图片添加特殊动画
  const heroImage = document.querySelector('.jumbotron img');
  if (heroImage) {
    heroImage.classList.add('fluent-animated', 'fluent-slide-left', 'fluent-duration-slow');
    heroImage.style.transitionDelay = '300ms';
  }
}); 