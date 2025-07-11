import { Editor } from '@toast-ui/editor';
import codeSyntaxHighlight from '@toast-ui/editor-plugin-code-syntax-highlight';

// 导入样式
import '@toast-ui/editor/dist/toastui-editor.css';
import '@toast-ui/editor/dist/theme/toastui-editor-dark.css';
import '@toast-ui/editor-plugin-code-syntax-highlight/dist/toastui-editor-plugin-code-syntax-highlight.css';
import '@/assets/styles/toastui-override.scss';

export default {
  install(app) {
    // 添加全局属性
    app.config.globalProperties.$toastui = {
      Editor,
      plugins: {
        codeSyntaxHighlight
      }
    };
  }
};
