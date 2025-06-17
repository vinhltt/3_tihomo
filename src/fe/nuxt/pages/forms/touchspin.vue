<template>
    <div>
        <ul class="flex space-x-2 rtl:space-x-reverse">
            <li>
                <a href="javascript:;" class="text-primary hover:underline">Forms</a>
            </li>
            <li class="before:content-['/'] ltr:before:mr-2 rtl:before:ml-2">
                <span>TouchSpin</span>
            </li>
        </ul>
        <div class="space-y-8 pt-5">
            <div class="panel flex items-center overflow-x-auto whitespace-nowrap p-3 text-primary">
                <div class="rounded-full bg-primary p-1.5 text-white ring-2 ring-primary/30 ltr:mr-3 rtl:ml-3">
                    <icon-bell />
                </div>
                <span class="ltr:mr-3 rtl:ml-3">Documentation: </span
                ><a href="https://www.npmjs.com/package/vue3-number-spinner" target="_blank" class="block hover:underline"
                    >https://www.npmjs.com/package/vue3-number-spinner</a
                >
            </div>

            <div class="grid grid-cols-1 gap-6 lg:grid-cols-2">
                <!-- Basic -->
                <div class="panel">
                    <div class="mb-5 flex items-center justify-between">
                        <h5 class="text-lg font-semibold dark:text-white-light">Button Spin</h5>
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
                        <div class="relative">
                            <div class="flex">
                                <button
                                    type="button"
                                    class="flex items-center justify-center border border-r-0 border-primary bg-primary px-3 font-semibold text-white ltr:rounded-l-md rtl:rounded-r-md"
                                    @click="value1--"
                                >
                                    <icon-minus class="w-5 h-5" />
                                </button>
                                <client-only>
                                    <number-spinner v-model="value1" :min="0" :max="100" :step="1" :keyStep="1" class="form-input rounded-none text-center" />
                                </client-only>
                                <button
                                    type="button"
                                    class="flex items-center justify-center border border-l-0 border-primary bg-primary px-3 font-semibold text-white ltr:rounded-r-md rtl:rounded-l-md"
                                    @click="value1++"
                                >
                                    <icon-plus />
                                </button>
                            </div>
                        </div>
                    </div>
                    <template v-if="codeArr.includes('code1')">
                        <highlight>
                            <pre>
  &lt;!-- basic --&gt;
  &lt;div class=&quot;flex&quot;&gt;
    &lt;button
      type=&quot;button&quot;
      class=&quot;bg-primary text-white flex justify-center items-center ltr:rounded-l-md rtl:rounded-r-md px-3 font-semibold border border-r-0 border-primary&quot;
      @click=&quot;value1--&quot;
    &gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
    &lt;number-spinner v-model=&quot;value1&quot; :min=&quot;0&quot; :max=&quot;100&quot; :step=&quot;1&quot; :keyStep=&quot;1&quot; class=&quot;form-input rounded-none text-center&quot; /&gt;
    &lt;button
      type=&quot;button&quot;
      class=&quot;bg-primary text-white flex justify-center items-center ltr:rounded-r-md rtl:rounded-l-md px-3 font-semibold border border-l-0 border-primary&quot;
      @click=&quot;value1++&quot;
    &gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
  &lt;/div&gt;

  &lt;!-- script --&gt;
  &lt;script lang=&quot;ts&quot; setup&gt;
    import { ref } from 'vue';
    import { NumberSpinner } from 'vue3-number-spinner';

    const value1 = ref(0);
  &lt;/script&gt;
  </pre
                            >
                        </highlight>
                    </template>
                </div>

                <!-- step of 5 -->
                <div class="panel">
                    <div class="mb-5 flex items-center justify-between">
                        <h5 class="text-lg font-semibold dark:text-white-light">Spin button with step of 5</h5>
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
                        <div class="relative">
                            <div class="flex">
                                <button
                                    type="button"
                                    class="flex items-center justify-center border border-r-0 border-primary bg-primary px-3 font-semibold text-white ltr:rounded-l-md rtl:rounded-r-md"
                                    @click="value2 -= 5"
                                >
                                    <icon-minus class="w-5 h-5" />
                                </button>
                                <client-only>
                                    <number-spinner v-model="value2" :min="0" :max="50" :step="5" :keyStep="5" class="form-input rounded-none text-center"
                                /></client-only>
                                <button
                                    type="button"
                                    class="flex items-center justify-center border border-l-0 border-primary bg-primary px-3 font-semibold text-white ltr:rounded-r-md rtl:rounded-l-md"
                                    @click="value2 += 5"
                                >
                                    <icon-plus />
                                </button>
                            </div>
                        </div>
                    </div>
                    <template v-if="codeArr.includes('code2')">
                        <highlight>
                            <pre>
  &lt;!-- step of 5 --&gt;
  &lt;div class=&quot;flex&quot;&gt;
    &lt;button
      type=&quot;button&quot;
      class=&quot;bg-primary text-white flex justify-center items-center ltr:rounded-l-md rtl:rounded-r-md px-3 font-semibold border border-r-0 border-primary&quot;
      @click=&quot;value2 -= 5&quot;
    &gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
    &lt;number-spinner v-model=&quot;value2&quot; :min=&quot;0&quot; :max=&quot;50&quot; :step=&quot;5&quot; :keyStep=&quot;5&quot; class=&quot;form-input rounded-none text-center&quot; /&gt;
    &lt;button
      type=&quot;button&quot;
      class=&quot;bg-primary text-white flex justify-center items-center ltr:rounded-r-md rtl:rounded-l-md px-3 font-semibold border border-l-0 border-primary&quot;
      @click=&quot;value2 += 5&quot;
    &gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
  &lt;/div&gt;

  &lt;!-- script --&gt;
  &lt;script lang=&quot;ts&quot; setup&gt;
    import { ref } from 'vue';
    import { NumberSpinner } from 'vue3-number-spinner';

    const value2 = ref(0);
  &lt;/script&gt;
  </pre
                            >
                        </highlight>
                    </template>
                </div>

                <!-- Wrapping value -->
                <div class="panel">
                    <div class="mb-5 flex items-center justify-between">
                        <h5 class="text-lg font-semibold dark:text-white-light">Wrapping value spin button</h5>
                        <a
                            class="font-semibold hover:text-gray-400 dark:text-gray-400 dark:hover:text-gray-600"
                            href="javascript:;"
                            @click="toggleCode('code3')"
                        >
                            <span class="flex items-center">
                                <icon-code class="me-2" />
                                Code
                            </span>
                        </a>
                    </div>
                    <div class="mb-5">
                        <div class="relative">
                            <div class="flex">
                                <button
                                    type="button"
                                    class="flex items-center justify-center border border-r-0 border-primary bg-primary px-3 font-semibold text-white ltr:rounded-l-md rtl:rounded-r-md"
                                    @click="value3 -= 1"
                                >
                                    <icon-plus />
                                </button>
                                <client-only>
                                    <number-spinner
                                        v-model="value3"
                                        :min="0"
                                        :max="21"
                                        :step="1"
                                        :keyStep="1"
                                        :circular="true"
                                        class="form-input rounded-none text-center"
                                /></client-only>
                                <button
                                    type="button"
                                    class="flex items-center justify-center border border-l-0 border-primary bg-primary px-3 font-semibold text-white ltr:rounded-r-md rtl:rounded-l-md"
                                    @click="value3 += 1"
                                >
                                    <icon-plus />
                                </button>
                            </div>
                        </div>
                    </div>
                    <template v-if="codeArr.includes('code3')">
                        <highlight>
                            <pre>
  &lt;!-- wrapping value --&gt;
  &lt;div class=&quot;flex&quot;&gt;
    &lt;button
      type=&quot;button&quot;
      class=&quot;bg-primary text-white flex justify-center items-center ltr:rounded-l-md rtl:rounded-r-md px-3 font-semibold border border-r-0 border-primary&quot;
      @click=&quot;value3 -= 1&quot;
    &gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
    &lt;number-spinner v-model=&quot;value3&quot; :min=&quot;0&quot; :max=&quot;21&quot; :step=&quot;1&quot; :keyStep=&quot;1&quot; :circular=&quot;true&quot; class=&quot;form-input rounded-none text-center&quot; /&gt;
    &lt;button
      type=&quot;button&quot;
      class=&quot;bg-primary text-white flex justify-center items-center ltr:rounded-r-md rtl:rounded-l-md px-3 font-semibold border border-l-0 border-primary&quot;
      @click=&quot;value3 += 1&quot;
    &gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
  &lt;/div&gt;

  &lt;!-- script --&gt;
  &lt;script lang=&quot;ts&quot; setup&gt;
    import { ref } from 'vue';
    import { NumberSpinner } from 'vue3-number-spinner';

    const value3 = ref(0);
  &lt;/script&gt;
  </pre
                            >
                        </highlight>
                    </template>
                </div>

                <!-- Size -->
                <div class="panel lg:row-span-2">
                    <div class="mb-5 flex items-center justify-between">
                        <h5 class="text-lg font-semibold dark:text-white-light">Size</h5>
                        <a
                            class="font-semibold hover:text-gray-400 dark:text-gray-400 dark:hover:text-gray-600"
                            href="javascript:;"
                            @click="toggleCode('code4')"
                        >
                            <span class="flex items-center">
                                <icon-code class="me-2" />
                                Code
                            </span>
                        </a>
                    </div>
                    <!-- large -->
                    <div class="mb-5">
                        <div class="mb-5">
                            <label for="sizeLarge">Large size</label>
                            <div class="flex">
                                <button
                                    type="button"
                                    class="flex items-center justify-center border border-r-0 border-primary bg-primary px-3 font-semibold text-white ltr:rounded-l-md rtl:rounded-r-md"
                                    @click="value4--"
                                >
                                    <icon-minus class="w-5 h-5" />
                                </button>
                                <client-only>
                                    <number-spinner
                                        v-model="value4"
                                        :min="0"
                                        :max="25"
                                        :step="1"
                                        :keyStep="1"
                                        class="form-input-lg form-input rounded-none text-center"
                                /></client-only>
                                <button
                                    type="button"
                                    class="flex items-center justify-center border border-l-0 border-primary bg-primary px-3 font-semibold text-white ltr:rounded-r-md rtl:rounded-l-md"
                                    @click="value4++"
                                >
                                    <icon-plus />
                                </button>
                            </div>
                        </div>

                        <!-- default -->
                        <div class="mb-5">
                            <label for="sizeDefault">Default size</label>
                            <div class="flex">
                                <button
                                    type="button"
                                    class="flex items-center justify-center border border-r-0 border-primary bg-primary px-3 font-semibold text-white ltr:rounded-l-md rtl:rounded-r-md"
                                    @click="value5--"
                                >
                                    <icon-minus class="w-5 h-5" />
                                </button>
                                <client-only>
                                    <number-spinner v-model="value5" :min="0" :max="25" :step="1" :keyStep="1" class="form-input rounded-none text-center"
                                /></client-only>
                                <button
                                    type="button"
                                    class="flex items-center justify-center border border-l-0 border-primary bg-primary px-3 font-semibold text-white ltr:rounded-r-md rtl:rounded-l-md"
                                    @click="value5++"
                                >
                                    <icon-plus />
                                </button>
                            </div>
                        </div>

                        <!-- small -->
                        <div>
                            <label for="sizeSmall">Small size</label>
                            <div class="flex">
                                <button
                                    type="button"
                                    class="flex items-center justify-center border border-r-0 border-primary bg-primary px-3 font-semibold text-white ltr:rounded-l-md rtl:rounded-r-md"
                                    @click="value6--"
                                >
                                    <icon-minus class="w-5 h-5" />
                                </button>
                                <client-only>
                                    <number-spinner
                                        v-model="value6"
                                        :min="0"
                                        :max="25"
                                        :step="1"
                                        :keyStep="1"
                                        class="form-input-sm form-input rounded-none text-center"
                                /></client-only>
                                <button
                                    type="button"
                                    class="flex items-center justify-center border border-l-0 border-primary bg-primary px-3 font-semibold text-white ltr:rounded-r-md rtl:rounded-l-md"
                                    @click="value6++"
                                >
                                    <icon-plus />
                                </button>
                            </div>
                        </div>
                    </div>
                    <template v-if="codeArr.includes('code4')">
                        <highlight>
                            <pre>
  &lt;!-- large --&gt;
  &lt;div class=&quot;flex&quot;&gt;
    &lt;button
      type=&quot;button&quot;
      class=&quot;bg-primary text-white flex justify-center items-center ltr:rounded-l-md rtl:rounded-r-md px-3 font-semibold border border-r-0 border-primary&quot;
      @click=&quot;value4--&quot;
    &gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
    &lt;number-spinner v-model=&quot;value4&quot; :min=&quot;0&quot; :max=&quot;25&quot; :step=&quot;1&quot; :keyStep=&quot;1&quot; class=&quot;form-input form-input-lg rounded-none text-center&quot; /&gt;
    &lt;button
      type=&quot;button&quot;
      class=&quot;bg-primary text-white flex justify-center items-center ltr:rounded-r-md rtl:rounded-l-md px-3 font-semibold border border-l-0 border-primary&quot;
      @click=&quot;value4++&quot;
    &gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
  &lt;/div&gt;

  &lt;!-- default --&gt;
  &lt;div class=&quot;flex&quot;&gt;
    &lt;button
      type=&quot;button&quot;
      class=&quot;bg-primary text-white flex justify-center items-center ltr:rounded-l-md rtl:rounded-r-md px-3 font-semibold border border-r-0 border-primary&quot;
      @click=&quot;value5--&quot;
    &gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
    &lt;number-spinner v-model=&quot;value5&quot; :min=&quot;0&quot; :max=&quot;25&quot; :step=&quot;1&quot; :keyStep=&quot;1&quot; class=&quot;form-input rounded-none text-center&quot; /&gt;
    &lt;button
      type=&quot;button&quot;
      class=&quot;bg-primary text-white flex justify-center items-center ltr:rounded-r-md rtl:rounded-l-md px-3 font-semibold border border-l-0 border-primary&quot;
      @click=&quot;value5++&quot;
    &gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
  &lt;/div&gt;

  &lt;!-- small --&gt;
  &lt;div class=&quot;flex&quot;&gt;
    &lt;button
      type=&quot;button&quot;
      class=&quot;bg-primary text-white flex justify-center items-center ltr:rounded-l-md rtl:rounded-r-md px-3 font-semibold border border-r-0 border-primary&quot;
      @click=&quot;value6--&quot;
    &gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
    &lt;number-spinner v-model=&quot;value6&quot; :min=&quot;0&quot; :max=&quot;25&quot; :step=&quot;1&quot; :keyStep=&quot;1&quot; class=&quot;form-input form-input-sm rounded-none text-center&quot; /&gt;
    &lt;button
      type=&quot;button&quot;
      class=&quot;bg-primary text-white flex justify-center items-center ltr:rounded-r-md rtl:rounded-l-md px-3 font-semibold border border-l-0 border-primary&quot;
      @click=&quot;value6++&quot;
    &gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
  &lt;/div&gt;

  &lt;!-- script --&gt;
  &lt;script lang=&quot;ts&quot; setup&gt;
    import { ref } from 'vue';
    import { NumberSpinner } from 'vue3-number-spinner';

    const value4 = ref(0);
    const value5 = ref(0);
    const value6 = ref(0);
  &lt;/script&gt;
  </pre
                            >
                        </highlight>
                    </template>
                </div>
                <!-- Inline Buttons -->
                <div class="panel">
                    <div class="mb-5 flex items-center justify-between">
                        <h5 class="text-lg font-semibold dark:text-white-light">Inline spin button</h5>
                        <a
                            class="font-semibold hover:text-gray-400 dark:text-gray-400 dark:hover:text-gray-600"
                            href="javascript:;"
                            @click="toggleCode('code5')"
                        >
                            <span class="flex items-center">
                                <icon-code class="me-2" />
                                Code
                            </span>
                        </a>
                    </div>
                    <div class="mb-5">
                        <div class="mb-5">
                            <div class="inline-flex">
                                <button
                                    type="button"
                                    class="flex items-center justify-center border border-r-0 border-primary bg-primary px-3 font-semibold text-white ltr:rounded-l-md rtl:rounded-r-md"
                                    @click="value7--"
                                >
                                    <icon-minus class="w-5 h-5" />
                                </button>
                                <client-only>
                                    <number-spinner v-model="value7" :min="0" :max="25" :step="1" :keyStep="1" class="form-input rounded-none text-center"
                                /></client-only>
                                <button
                                    type="button"
                                    class="flex items-center justify-center border border-l-0 border-primary bg-primary px-3 font-semibold text-white ltr:rounded-r-md rtl:rounded-l-md"
                                    @click="value7++"
                                >
                                    <icon-plus />
                                </button>
                            </div>
                        </div>
                    </div>
                    <template v-if="codeArr.includes('code5')">
                        <highlight>
                            <pre>
  &lt;!-- inline buttons --&gt;
  &lt;div class=&quot;inline-flex&quot;&gt;
    &lt;button
      type=&quot;button&quot;
      class=&quot;bg-primary text-white flex justify-center items-center ltr:rounded-l-md rtl:rounded-r-md px-3 font-semibold border border-r-0 border-primary&quot;
      @click=&quot;value7--&quot;
    &gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
    &lt;number-spinner v-model=&quot;value7&quot; :min=&quot;0&quot; :max=&quot;25&quot; :step=&quot;1&quot; :keyStep=&quot;1&quot; class=&quot;form-input rounded-none text-center&quot; /&gt;
    &lt;button
      type=&quot;button&quot;
      class=&quot;bg-primary text-white flex justify-center items-center ltr:rounded-r-md rtl:rounded-l-md px-3 font-semibold border border-l-0 border-primary&quot;
      @click=&quot;value7++&quot;
    &gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
  &lt;/div&gt;

  &lt;!-- script --&gt;
  &lt;script lang=&quot;ts&quot; setup&gt;
    import { ref } from 'vue';
    import { NumberSpinner } from 'vue3-number-spinner';

    const value7 = ref(0);
  &lt;/script&gt;
  </pre
                            >
                        </highlight>
                    </template>
                </div>

                <!-- Vertical Buttons -->
                <div class="panel lg:row-span-2">
                    <div class="mb-5 flex items-center justify-between">
                        <h5 class="text-lg font-semibold dark:text-white-light">Vertical spin button</h5>
                        <a
                            class="font-semibold hover:text-gray-400 dark:text-gray-400 dark:hover:text-gray-600"
                            href="javascript:;"
                            @click="toggleCode('code6')"
                        >
                            <span class="flex items-center">
                                <icon-code class="me-2" />
                                Code
                            </span>
                        </a>
                    </div>
                    <div class="mb-5">
                        <div class="mb-5">
                            <div class="inline-flex w-[50px] flex-col">
                                <button
                                    type="button"
                                    class="flex items-center justify-center rounded-t-md border border-b-0 border-primary bg-primary p-3 font-semibold text-white"
                                    @click="value8--"
                                >
                                    <icon-minus class="w-5 h-5" />
                                </button>
                                <client-only>
                                    <number-spinner
                                        v-model="value8"
                                        :min="0"
                                        :max="25"
                                        :step="1"
                                        :keyStep="1"
                                        :vertical="true"
                                        class="form-input rounded-none px-2 text-center"
                                /></client-only>
                                <button
                                    type="button"
                                    class="flex items-center justify-center rounded-b-md border border-t-0 border-primary bg-primary p-3 font-semibold text-white"
                                    @click="value8++"
                                >
                                    <icon-plus />
                                </button>
                            </div>
                        </div>
                    </div>
                    <template v-if="codeArr.includes('code6')">
                        <highlight>
                            <pre>
  &lt;!-- vertical buttons --&gt;
  &lt;div class=&quot;inline-flex flex-col w-[50px]&quot;&gt;
    &lt;button type=&quot;button&quot; class=&quot;bg-primary text-white flex justify-center items-center rounded-t-md p-3 font-semibold border border-b-0 border-primary&quot; @click=&quot;value8--&quot;&gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
    &lt;number-spinner v-model=&quot;value8&quot; :min=&quot;0&quot; :max=&quot;25&quot; :step=&quot;1&quot; :keyStep=&quot;1&quot; :vertical=&quot;true&quot; class=&quot;form-input rounded-none text-center px-2&quot; /&gt;
    &lt;button type=&quot;button&quot; class=&quot;bg-primary text-white flex justify-center items-center rounded-b-md p-3 font-semibold border border-t-0 border-primary&quot; @click=&quot;value8++&quot;&gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
  &lt;/div&gt;

  &lt;!-- script --&gt;
  &lt;script lang=&quot;ts&quot; setup&gt;
    import { ref } from 'vue';
    import { NumberSpinner } from 'vue3-number-spinner';

    const value8 = ref(0);
  &lt;/script&gt;
  </pre
                            >
                        </highlight>
                    </template>
                </div>

                <!-- text with spin  -->
                <div class="panel">
                    <div class="mb-5 flex items-center justify-between">
                        <h5 class="text-lg font-semibold dark:text-white-light">Text with spin button</h5>
                        <a
                            class="font-semibold hover:text-gray-400 dark:text-gray-400 dark:hover:text-gray-600"
                            href="javascript:;"
                            @click="toggleCode('code7')"
                        >
                            <span class="flex items-center">
                                <icon-code class="me-2" />
                                Code
                            </span>
                        </a>
                    </div>
                    <div class="mb-5">
                        <div class="relative">
                            <div class="flex">
                                <button
                                    type="button"
                                    class="flex items-center justify-center border border-r-0 border-primary bg-primary px-3 font-semibold text-white ltr:rounded-l-md rtl:rounded-r-md"
                                    @click="value9--"
                                >
                                    <icon-minus class="w-5 h-5" />
                                </button>
                                <client-only>
                                    <number-spinner
                                        v-model="value9"
                                        :format="dayFormatter"
                                        :min="0"
                                        :max="6"
                                        :step="1"
                                        :keyStep="1"
                                        class="form-input rounded-none text-center"
                                /></client-only>
                                <button
                                    type="button"
                                    class="flex items-center justify-center border border-l-0 border-primary bg-primary px-3 font-semibold text-white ltr:rounded-r-md rtl:rounded-l-md"
                                    @click="value9++"
                                >
                                    <icon-plus />
                                </button>
                            </div>
                        </div>
                    </div>
                    <template v-if="codeArr.includes('code7')">
                        <highlight>
                            <pre>
  &lt;!-- text with spin button --&gt;
  &lt;div class=&quot;flex&quot;&gt;
    &lt;button
      type=&quot;button&quot;
      class=&quot;bg-primary text-white flex justify-center items-center ltr:rounded-l-md rtl:rounded-r-md px-3 font-semibold border border-r-0 border-primary&quot;
      @click=&quot;value9--&quot;
    &gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
    &lt;number-spinner v-model=&quot;value9&quot; :format=&quot;dayFormatter&quot; :min=&quot;0&quot; :max=&quot;6&quot; :step=&quot;1&quot; :keyStep=&quot;1&quot; class=&quot;form-input rounded-none text-center&quot; /&gt;
    &lt;button
      type=&quot;button&quot;
      class=&quot;bg-primary text-white flex justify-center items-center ltr:rounded-r-md rtl:rounded-l-md px-3 font-semibold border border-l-0 border-primary&quot;
      @click=&quot;value9++&quot;
    &gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
  &lt;/div&gt;

  &lt;!-- script --&gt;
  &lt;script lang=&quot;ts&quot; setup&gt;
    import { ref } from 'vue';
    import { NumberSpinner } from 'vue3-number-spinner';

    const value9 = ref(0);
    const days = ref(['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']);

    const dayFormatter = (value) =&gt; {
      return days.value[value];
    };
  &lt;/script&gt;
  </pre
                            >
                        </highlight>
                    </template>
                </div>
                <!-- Change Button Class -->
                <div class="panel">
                    <div class="mb-5 flex items-center justify-between">
                        <h5 class="text-lg font-semibold dark:text-white-light">Change button class</h5>
                        <a
                            class="font-semibold hover:text-gray-400 dark:text-gray-400 dark:hover:text-gray-600"
                            href="javascript:;"
                            @click="toggleCode('code8')"
                        >
                            <span class="flex items-center">
                                <icon-code class="me-2" />
                                Code
                            </span>
                        </a>
                    </div>
                    <div class="mb-5">
                        <div class="relative">
                            <div class="flex">
                                <button
                                    type="button"
                                    class="flex items-center justify-center border border-r-0 border-danger bg-danger px-3 font-semibold text-white ltr:rounded-l-md rtl:rounded-r-md"
                                    @click="value10--"
                                >
                                    <icon-minus class="w-5 h-5" />
                                </button>
                                <client-only>
                                    <number-spinner v-model="value10" :min="0" :max="25" :step="1" :keyStep="1" class="form-input rounded-none text-center"
                                /></client-only>
                                <button
                                    type="button"
                                    class="flex items-center justify-center border border-l-0 border-warning bg-warning px-3 font-semibold text-white ltr:rounded-r-md rtl:rounded-l-md"
                                    @click="value10++"
                                >
                                    <icon-plus />
                                </button>
                            </div>
                        </div>
                    </div>
                    <template v-if="codeArr.includes('code8')">
                        <highlight>
                            <pre>
  &lt;!-- basic --&gt;
  &lt;div class=&quot;flex&quot;&gt;
    &lt;button
      type=&quot;button&quot;
      class=&quot;bg-danger text-white flex justify-center items-center ltr:rounded-l-md rtl:rounded-r-md px-3 font-semibold border border-r-0 border-danger&quot;
      @click=&quot;value10--&quot;
    &gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
    &lt;number-spinner v-model=&quot;value10&quot; :min=&quot;0&quot; :max=&quot;25&quot; :step=&quot;1&quot; :keyStep=&quot;1&quot; class=&quot;form-input rounded-none text-center&quot; /&gt;
    &lt;button
      type=&quot;button&quot;
      class=&quot;bg-warning text-white flex justify-center items-center ltr:rounded-r-md rtl:rounded-l-md px-3 font-semibold border border-l-0 border-warning&quot;
      @click=&quot;value10++&quot;
    &gt;
      &lt;svg&gt; ... &lt;/svg&gt;
    &lt;/button&gt;
  &lt;/div&gt;

  &lt;!-- script --&gt;
  &lt;script lang=&quot;ts&quot; setup&gt;
    import { ref } from 'vue';
    import { NumberSpinner } from 'vue3-number-spinner';

    const value10 = ref(0);
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
    import { ref } from 'vue';
    import highlight from '@/components/plugins/highlight.vue';
    import codePreview from '@/composables/codePreview';
    import { NumberSpinner } from 'vue3-number-spinner';
    useHead({ title: 'Touchspin' });

    const { codeArr, toggleCode } = codePreview();

    const value1 = ref(0);
    const value2 = ref(0);
    const value3 = ref(0);
    const value4 = ref(0);
    const value5 = ref(0);
    const value6 = ref(0);
    const value7 = ref(0);
    const value8 = ref(0);
    const value9 = ref(0);
    const value10 = ref(0);
    const days = ref(['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']);

    const dayFormatter = (value: any) => {
        return days.value[value];
    };
</script>
