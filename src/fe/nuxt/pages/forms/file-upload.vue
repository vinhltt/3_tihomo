<template>
    <div>
        <ul class="flex space-x-2 rtl:space-x-reverse">
            <li>
                <a href="javascript:;" class="text-primary hover:underline">Forms</a>
            </li>
            <li class="before:content-['/'] ltr:before:mr-2 rtl:before:ml-2">
                <span>File Upload</span>
            </li>
        </ul>
        <div class="space-y-8 pt-5">
            <div class="panel flex items-center overflow-x-auto whitespace-nowrap p-3 text-primary">
                <div class="rounded-full bg-primary p-1.5 text-white ring-2 ring-primary/30 ltr:mr-3 rtl:ml-3">
                    <icon-bell />
                </div>
                <span class="ltr:mr-3 rtl:ml-3">Documentation: </span>
                <a href="https://www.npmjs.com/package/file-upload-with-preview" target="_blank" class="block hover:underline">
                    https://www.npmjs.com/package/file-upload-with-preview
                </a>
            </div>

            <div class="grid grid-cols-1 gap-6 lg:grid-cols-2">
                <!-- Single File -->
                <div class="panel">
                    <div class="mb-5 flex items-center justify-between">
                        <h5 class="text-lg font-semibold dark:text-white-light">Single File Upload</h5>
                        <a
                            class="font-semibold hover:text-gray-400 dark:text-gray-400 dark:hover:text-gray-600"
                            href="javascript:;"
                            @click="toggleCode('code1')"
                        >
                            <span class="flex items-center">
                                <icon-code class="me-2" />
                                Code
                            </span>
                        </a>
                    </div>
                    <div class="mb-5">
                        <div class="custom-file-container" data-upload-id="myFirstImage">
                            <div class="label-container">
                                <label>Upload </label> <a href="javascript:;" class="custom-file-container__image-clear" title="Clear Image">×</a>
                            </div>
                            <label class="custom-file-container__custom-file">
                                <input type="file" class="custom-file-container__custom-file__custom-file-input" accept="image/*" />
                                <input type="hidden" name="MAX_FILE_SIZE" value="10485760" />
                                <span class="custom-file-container__custom-file__custom-file-control ltr:pr-20 rtl:pl-20"></span>
                            </label>
                            <div class="custom-file-container__image-preview"></div>
                        </div>
                    </div>
                    <template v-if="codeArr.includes('code1')">
                        <highlight>
                            <pre>
  &lt;!-- single file --&gt;
  &lt;div class=&quot;custom-file-container&quot; data-upload-id=&quot;myFirstImage&quot;&gt;
    &lt;div class=&quot;label-container&quot;&gt;&lt;label&gt;Upload &lt;/label&gt; &lt;a href=&quot;javascript:;&quot; class=&quot;custom-file-container__image-clear&quot; title=&quot;Clear Image&quot;&gt;&times;&lt;/a&gt;&lt;/div&gt;
    &lt;label class=&quot;custom-file-container__custom-file&quot;&gt;
      &lt;input type=&quot;file&quot; class=&quot;custom-file-container__custom-file__custom-file-input&quot; accept=&quot;image/*&quot; /&gt;
      &lt;input type=&quot;hidden&quot; name=&quot;MAX_FILE_SIZE&quot; value=&quot;10485760&quot; /&gt;
      &lt;span class=&quot;custom-file-container__custom-file__custom-file-control ltr:pr-20 rtl:pl-20&quot;&gt;&lt;/span&gt;
    &lt;/label&gt;
    &lt;div class=&quot;custom-file-container__image-preview&quot;&gt;&lt;/div&gt;
  &lt;/div&gt;

  &lt;!-- script --&gt;
  &lt;script lang=&quot;ts&quot; setup&gt;
    import { onMounted } from 'vue';
    import FileUploadWithPreview from 'file-upload-with-preview';
    import 'file-upload-with-preview/dist/file-upload-with-preview.min.css';
    import '../../assets/css/file-upload-with-preview.min.css';

    onMounted(() =&gt; {
      // single image upload
      new FileUploadWithPreview('myFirstImage', {
        images: {
          baseImage: '/assets/images/file-preview.svg',
          backgroundImage: '',
        },
      });
    });
  &lt;/script&gt;
  </pre
                            >
                        </highlight>
                    </template>
                </div>
                <!-- Multiple File -->
                <div class="panel">
                    <div class="mb-5 flex items-center justify-between">
                        <h5 class="text-lg font-semibold dark:text-white-light">Multiple File</h5>
                        <a
                            class="font-semibold hover:text-gray-400 dark:text-gray-400 dark:hover:text-gray-600"
                            href="javascript:;"
                            @click="toggleCode('code2')"
                        >
                            <span class="flex items-center">
                                <icon-code class="me-2" />
                                Code
                            </span>
                        </a>
                    </div>
                    <div class="mb-5">
                        <div class="custom-file-container" data-upload-id="mySecondImage">
                            <div class="label-container">
                                <label>Upload </label> <a href="javascript:;" class="custom-file-container__image-clear" title="Clear Image">×</a>
                            </div>
                            <label class="custom-file-container__custom-file">
                                <input type="file" class="custom-file-container__custom-file__custom-file-input" multiple />
                                <input type="hidden" name="MAX_FILE_SIZE" value="10485760" />
                                <span class="custom-file-container__custom-file__custom-file-control ltr:pr-20 rtl:pl-20"></span>
                            </label>
                            <div class="custom-file-container__image-preview"></div>
                        </div>
                    </div>
                    <template v-if="codeArr.includes('code2')">
                        <highlight>
                            <pre>
  &lt;!-- multiple file --&gt;
  &lt;div class=&quot;custom-file-container&quot; data-upload-id=&quot;mySecondImage&quot;&gt;
    &lt;div class=&quot;label-container&quot;&gt;&lt;label&gt;Upload &lt;/label&gt; &lt;a href=&quot;javascript:;&quot; class=&quot;custom-file-container__image-clear&quot; title=&quot;Clear Image&quot;&gt;&times;&lt;/a&gt;&lt;/div&gt;
    &lt;label class=&quot;custom-file-container__custom-file&quot;&gt;
      &lt;input type=&quot;file&quot; class=&quot;custom-file-container__custom-file__custom-file-input&quot; multiple /&gt;
      &lt;input type=&quot;hidden&quot; name=&quot;MAX_FILE_SIZE&quot; value=&quot;10485760&quot; /&gt;
      &lt;span class=&quot;custom-file-container__custom-file__custom-file-control ltr:pr-20 rtl:pl-20&quot;&gt;&lt;/span&gt;
    &lt;/label&gt;
    &lt;div class=&quot;custom-file-container__image-preview&quot;&gt;&lt;/div&gt;
  &lt;/div&gt;

  &lt;!-- script --&gt;
  &lt;script lang=&quot;ts&quot; setup&gt;
    import { onMounted } from 'vue';
    import FileUploadWithPreview from 'file-upload-with-preview';
    import 'file-upload-with-preview/dist/file-upload-with-preview.min.css';
    import '../../assets/css/file-upload-with-preview.min.css';

    onMounted(() =&gt; {
      // multiple image upload
      new FileUploadWithPreview('mySecondImage', {
        images: {
          baseImage: '/assets/images/file-preview.svg',
          backgroundImage: '',
        },
        multiple: true,
      });
    });
  &lt;/script&gt;
  </pre
                            >
                        </highlight>
                    </template>
                </div>
            </div>
        </div>
    </div>
</template>
<script lang="ts" setup>
    import { onMounted } from 'vue';
    import highlight from '@/components/plugins/highlight.vue';
    import codePreview from '@/composables/codePreview';
    useHead({ title: 'File Upload' });

    const { codeArr, toggleCode } = codePreview();

    onMounted(async () => {
        const fileupload = await import('file-upload-with-preview');
        let FileUploadWithPreview = fileupload.default;

        // single image upload
        new FileUploadWithPreview('myFirstImage', {
            images: {
                baseImage: '/assets/images/file-preview.svg',
                backgroundImage: '',
            },
        });

        // multiple image upload
        new FileUploadWithPreview('mySecondImage', {
            images: {
                baseImage: '/assets/images/file-preview.svg',
                backgroundImage: '',
            },
            multiple: true,
        });
    });
</script>
