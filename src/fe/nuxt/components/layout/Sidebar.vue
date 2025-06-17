<template>
    <div :class="{ 'dark text-white-dark': store.semidark }">
        <nav class="sidebar fixed bottom-0 top-0 z-50 h-full min-h-screen w-[260px] shadow-[5px_0_25px_0_rgba(94,92,154,0.1)] transition-all duration-300">
            <div class="h-full bg-white dark:bg-[#0e1726]">
                <div class="flex items-center justify-between px-4 py-3">
                    <NuxtLink to="/" class="main-logo flex shrink-0 items-center">
                        <img class="ml-[5px] w-8 flex-none" src="/assets/images/logo.svg" alt="" />
                        <span class="align-middle text-2xl font-semibold ltr:ml-1.5 rtl:mr-1.5 dark:text-white-light lg:inline">VRISTO</span>
                    </NuxtLink>
                    <a
                        href="javascript:;"
                        class="collapse-icon flex h-8 w-8 items-center rounded-full transition duration-300 hover:bg-gray-500/10 hover:text-primary rtl:rotate-180 dark:text-white-light dark:hover:bg-dark-light/10"
                        @click="store.toggleSidebar()"
                    >
                        <icon-carets-down class="m-auto rotate-90" />
                    </a>
                </div>
                <client-only>
                    <perfect-scrollbar
                        :options="{
                            swipeEasing: true,
                            wheelPropagation: false,
                        }"
                        class="relative h-[calc(100vh-80px)]"
                    >
                        <ul class="relative space-y-0.5 p-4 py-0 font-semibold">

                            <li class="nav-item">
                                <NuxtLink to="/apps/accounts" class="group" @click="toggleMobileMenu">
                                    <div class="flex items-center">
                                        <icon-credit-card class="shrink-0 group-hover:!text-primary" />

                                        <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">
                                            Accounts
                                        </span>
                                    </div>
                                </NuxtLink>
                            </li>

                            <li class="nav-item">
                                <NuxtLink to="/apps/transactions" class="group" @click="toggleMobileMenu">
                                    <div class="flex items-center">
                                        <icon-dollar-sign class="shrink-0 group-hover:!text-primary" />

                                        <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">
                                            Transactions
                                        </span>
                                    </div>
                                </NuxtLink>
                            </li>

                            <li class="nav-item">
                                <NuxtLink to="/apps/recurring-transactions" class="group" @click="toggleMobileMenu">
                                    <div class="flex items-center">
                                        <icon-refresh class="shrink-0 group-hover:!text-primary" />

                                        <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">
                                            Recurring Transactions
                                        </span>
                                    </div>
                                </NuxtLink>
                            </li>
                            <!-- <li class="menu nav-item">
                                <button
                                    type="button"
                                    class="nav-link group w-full"
                                    :class="{ active: activeDropdown === 'dashboard' }"
                                    @click="activeDropdown === 'dashboard' ? (activeDropdown = null) : (activeDropdown = 'dashboard')"
                                >
                                    <div class="flex items-center">
                                        <icon-menu-dashboard class="shrink-0 group-hover:!text-primary" />
                                        <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">
                                            {{ $t('dashboard') }}
                                        </span>
                                    </div>
                                    <div :class="{ '-rotate-90 rtl:rotate-90': activeDropdown !== 'dashboard' }">
                                        <icon-caret-down />
                                    </div>
                                </button>
                                <vue-collapsible :isOpen="activeDropdown === 'dashboard'">
                                    <ul class="sub-menu text-gray-500">
                                        <li>
                                            <NuxtLink to="/" @click="toggleMobileMenu">{{ $t('sales') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/analytics" @click="toggleMobileMenu">{{ $t('analytics') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/finance" @click="toggleMobileMenu">{{ $t('finance') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/crypto" @click="toggleMobileMenu">{{ $t('crypto') }}</NuxtLink>
                                        </li>
                                    </ul>
                                </vue-collapsible>
                            </li>

                            <h2 class="-mx-4 mb-1 flex items-center bg-white-light/30 px-7 py-3 font-extrabold uppercase dark:bg-dark dark:bg-opacity-[0.08]">
                                <icon-minus class="hidden h-5 w-4 flex-none" />
                                <span>{{ $t('apps') }}</span>
                            </h2>

                            <li class="nav-item">
                                <ul>
                                    <li class="nav-item">
                                        <NuxtLink to="/apps/chat" class="group" @click="toggleMobileMenu">
                                            <div class="flex items-center">
                                                <icon-menu-chat class="shrink-0 group-hover:!text-primary" />

                                                <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{
                                                    $t('chat')
                                                }}</span>
                                            </div>
                                        </NuxtLink>
                                    </li>
                                    <li class="nav-item">
                                        <NuxtLink to="/apps/mailbox" class="group" @click="toggleMobileMenu">
                                            <div class="flex items-center">
                                                <icon-menu-mailbox class="shrink-0 group-hover:!text-primary" />

                                                <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{
                                                    $t('mailbox')
                                                }}</span>
                                            </div>
                                        </NuxtLink>
                                    </li>
                                    <li class="nav-item">
                                        <NuxtLink to="/apps/todolist" class="group" @click="toggleMobileMenu">
                                            <div class="flex items-center">
                                                <icon-menu-todo class="shrink-0 group-hover:!text-primary" />

                                                <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{
                                                    $t('todo_list')
                                                }}</span>
                                            </div>
                                        </NuxtLink>
                                    </li>
                                    <li class="nav-item">
                                        <NuxtLink to="/apps/notes" class="group" @click="toggleMobileMenu">
                                            <div class="flex items-center">
                                                <icon-menu-notes class="shrink-0 group-hover:!text-primary" />

                                                <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{
                                                    $t('notes')
                                                }}</span>
                                            </div>
                                        </NuxtLink>
                                    </li>
                                    <li class="nav-item">
                                        <NuxtLink to="/apps/scrumboard" class="group" @click="toggleMobileMenu">
                                            <div class="flex items-center">
                                                <icon-menu-scrumboard class="shrink-0 group-hover:!text-primary" />

                                                <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{
                                                    $t('scrumboard')
                                                }}</span>
                                            </div>
                                        </NuxtLink>
                                    </li>
                                    <li class="nav-item">
                                        <NuxtLink to="/apps/contacts" class="group" @click="toggleMobileMenu">
                                            <div class="flex items-center">
                                                <icon-menu-contacts class="shrink-0 group-hover:!text-primary" />

                                                <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{
                                                    $t('contacts')
                                                }}</span>
                                            </div>
                                        </NuxtLink>
                                    </li>

                                    <li class="menu nav-item">
                                        <button
                                            type="button"
                                            class="nav-link group w-full"
                                            :class="{ active: activeDropdown === 'invoice' }"
                                            @click="activeDropdown === 'invoice' ? (activeDropdown = null) : (activeDropdown = 'invoice')"
                                        >
                                            <div class="flex items-center">
                                                <icon-menu-invoice class="shrink-0 group-hover:!text-primary" />

                                                <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{
                                                    $t('invoice')
                                                }}</span>
                                            </div>
                                            <div :class="{ '-rotate-90 rtl:rotate-90': activeDropdown !== 'invoice' }">
                                                <icon-caret-down />
                                            </div>
                                        </button>
                                        <vue-collapsible :isOpen="activeDropdown === 'invoice'">
                                            <ul class="sub-menu text-gray-500">
                                                <li>
                                                    <NuxtLink to="/apps/invoice/list" @click="toggleMobileMenu">{{ $t('list') }}</NuxtLink>
                                                </li>
                                                <li>
                                                    <NuxtLink to="/apps/invoice/preview" @click="toggleMobileMenu">{{ $t('preview') }}</NuxtLink>
                                                </li>
                                                <li>
                                                    <NuxtLink to="/apps/invoice/add" @click="toggleMobileMenu">{{ $t('add') }}</NuxtLink>
                                                </li>
                                                <li>
                                                    <NuxtLink to="/apps/invoice/edit" @click="toggleMobileMenu">{{ $t('edit') }}</NuxtLink>
                                                </li>
                                            </ul>
                                        </vue-collapsible>
                                    </li>

                                    <li class="nav-item">
                                        <NuxtLink to="/apps/calendar" class="group" @click="toggleMobileMenu">
                                            <div class="flex items-center">
                                                <icon-menu-calendar class="shrink-0 group-hover:!text-primary" />

                                                <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{
                                                    $t('calendar')
                                                }}</span>
                                            </div>
                                        </NuxtLink>
                                    </li>
                                </ul>
                            </li>

                            <h2 class="-mx-4 mb-1 flex items-center bg-white-light/30 px-7 py-3 font-extrabold uppercase dark:bg-dark dark:bg-opacity-[0.08]">
                                <icon-minus class="hidden h-5 w-4 flex-none" />
                                <span>{{ $t('user_interface') }}</span>
                            </h2>

                            <li class="menu nav-item">
                                <button
                                    type="button"
                                    class="nav-link group w-full"
                                    :class="{ active: activeDropdown === 'components' }"
                                    @click="activeDropdown === 'components' ? (activeDropdown = null) : (activeDropdown = 'components')"
                                >
                                    <div class="flex items-center">
                                        <icon-menu-components class="shrink-0 group-hover:!text-primary" />

                                        <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{
                                            $t('components')
                                        }}</span>
                                    </div>
                                    <div :class="{ '-rotate-90 rtl:rotate-90': activeDropdown !== 'components' }">
                                        <icon-caret-down />
                                    </div>
                                </button>
                                <vue-collapsible :isOpen="activeDropdown === 'components'">
                                    <ul class="sub-menu text-gray-500">
                                        <li>
                                            <NuxtLink to="/components/tabs" @click="toggleMobileMenu">{{ $t('tabs') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/components/accordions" @click="toggleMobileMenu">{{ $t('accordions') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/components/modals" @click="toggleMobileMenu">{{ $t('modals') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/components/cards" @click="toggleMobileMenu">{{ $t('cards') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/components/carousel" @click="toggleMobileMenu">{{ $t('carousel') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/components/countdown" @click="toggleMobileMenu">{{ $t('countdown') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/components/counter" @click="toggleMobileMenu">{{ $t('counter') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/components/sweetalert" @click="toggleMobileMenu">{{ $t('sweet_alerts') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/components/timeline" @click="toggleMobileMenu">{{ $t('timeline') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/components/notifications" @click="toggleMobileMenu">{{ $t('notifications') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/components/media-object" @click="toggleMobileMenu">{{ $t('media_object') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/components/list-group" @click="toggleMobileMenu">{{ $t('list_group') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/components/pricing-table" @click="toggleMobileMenu">{{ $t('pricing_tables') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/components/lightbox" @click="toggleMobileMenu">{{ $t('lightbox') }}</NuxtLink>
                                        </li>
                                    </ul>
                                </vue-collapsible>
                            </li>

                            <li class="menu nav-item">
                                <button
                                    type="button"
                                    class="nav-link group w-full"
                                    :class="{ active: activeDropdown === 'elements' }"
                                    @click="activeDropdown === 'elements' ? (activeDropdown = null) : (activeDropdown = 'elements')"
                                >
                                    <div class="flex items-center">
                                        <icon-menu-elements class="shrink-0 group-hover:!text-primary" />

                                        <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{
                                            $t('elements')
                                        }}</span>
                                    </div>
                                    <div :class="{ '-rotate-90 rtl:rotate-90': activeDropdown !== 'elements' }">
                                        <icon-caret-down />
                                    </div>
                                </button>
                                <vue-collapsible :isOpen="activeDropdown === 'elements'">
                                    <ul class="sub-menu text-gray-500">
                                        <li>
                                            <NuxtLink to="/elements/alerts" @click="toggleMobileMenu">{{ $t('alerts') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/elements/avatar" @click="toggleMobileMenu">{{ $t('avatar') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/elements/badges" @click="toggleMobileMenu">{{ $t('badges') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/elements/breadcrumbs" @click="toggleMobileMenu">{{ $t('breadcrumbs') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/elements/buttons" @click="toggleMobileMenu">{{ $t('buttons') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/elements/buttons-group" @click="toggleMobileMenu">{{ $t('button_groups') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/elements/color-library" @click="toggleMobileMenu">{{ $t('color_library') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/elements/dropdown" @click="toggleMobileMenu">{{ $t('dropdown') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/elements/infobox" @click="toggleMobileMenu">{{ $t('infobox') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/elements/jumbotron" @click="toggleMobileMenu">{{ $t('jumbotron') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/elements/loader" @click="toggleMobileMenu">{{ $t('loader') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/elements/pagination" @click="toggleMobileMenu">{{ $t('pagination') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/elements/popovers" @click="toggleMobileMenu">{{ $t('popovers') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/elements/progress-bar" @click="toggleMobileMenu">{{ $t('progress_bar') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/elements/search" @click="toggleMobileMenu">{{ $t('search') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/elements/tooltips" @click="toggleMobileMenu">{{ $t('tooltips') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/elements/treeview" @click="toggleMobileMenu">{{ $t('treeview') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/elements/typography" @click="toggleMobileMenu">{{ $t('typography') }}</NuxtLink>
                                        </li>
                                    </ul>
                                </vue-collapsible>
                            </li>

                            <li class="menu nav-item">
                                <NuxtLink to="/charts" class="nav-link group" @click="toggleMobileMenu">
                                    <div class="flex items-center">
                                        <icon-menu-charts class="shrink-0 group-hover:!text-primary" />

                                        <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{
                                            $t('charts')
                                        }}</span>
                                    </div>
                                </NuxtLink>
                            </li>

                            <li class="menu nav-item">
                                <NuxtLink to="/widgets" class="nav-link group" @click="toggleMobileMenu">
                                    <div class="flex items-center">
                                        <icon-menu-widgets class="shrink-0 group-hover:!text-primary" />

                                        <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{
                                            $t('widgets')
                                        }}</span>
                                    </div>
                                </NuxtLink>
                            </li>

                            <li class="menu nav-item">
                                <NuxtLink to="/font-icons" class="nav-link group" @click="toggleMobileMenu">
                                    <div class="flex items-center">
                                        <icon-menu-font-icons class="shrink-0 group-hover:!text-primary" />

                                        <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{
                                            $t('font_icons')
                                        }}</span>
                                    </div>
                                </NuxtLink>
                            </li>

                            <li class="menu nav-item">
                                <NuxtLink to="/dragndrop" class="nav-link group" @click="toggleMobileMenu">
                                    <div class="flex items-center">
                                        <icon-menu-drag-and-drop class="shrink-0 group-hover:!text-primary" />

                                        <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{
                                            $t('drag_and_drop')
                                        }}</span>
                                    </div>
                                </NuxtLink>
                            </li>

                            <h2 class="-mx-4 mb-1 flex items-center bg-white-light/30 px-7 py-3 font-extrabold uppercase dark:bg-dark dark:bg-opacity-[0.08]">
                                <icon-minus class="hidden h-5 w-4 flex-none" />
                                <span>{{ $t('tables_and_forms') }}</span>
                            </h2>

                            <li class="menu nav-item">
                                <NuxtLink to="/tables" class="nav-link group" @click="toggleMobileMenu">
                                    <div class="flex items-center">
                                        <icon-menu-tables class="shrink-0 group-hover:!text-primary" />

                                        <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{
                                            $t('tables')
                                        }}</span>
                                    </div>
                                </NuxtLink>
                            </li>

                            <li class="menu nav-item">
                                <button
                                    type="button"
                                    class="nav-link group w-full"
                                    :class="{ active: activeDropdown === 'datatables' }"
                                    @click="activeDropdown === 'datatables' ? (activeDropdown = null) : (activeDropdown = 'datatables')"
                                >
                                    <div class="flex items-center">
                                        <icon-menu-datatables class="shrink-0 group-hover:!text-primary" />

                                        <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{
                                            $t('datatables')
                                        }}</span>
                                    </div>
                                    <div :class="{ '-rotate-90 rtl:rotate-90': activeDropdown !== 'datatables' }">
                                        <icon-caret-down />
                                    </div>
                                </button>
                                <vue-collapsible :isOpen="activeDropdown === 'datatables'">
                                    <ul class="sub-menu text-gray-500">
                                        <li>
                                            <NuxtLink to="/datatables/basic" @click="toggleMobileMenu">{{ $t('basic') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/datatables/advanced" @click="toggleMobileMenu">{{ $t('advanced') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/datatables/skin" @click="toggleMobileMenu">{{ $t('skin') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/datatables/order-sorting" @click="toggleMobileMenu">{{ $t('order_sorting') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/datatables/columns-filter" @click="toggleMobileMenu">{{ $t('columns_filter') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/datatables/multi-column" @click="toggleMobileMenu">{{ $t('multi_column') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/datatables/multiple-tables" @click="toggleMobileMenu">{{ $t('multiple_tables') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/datatables/alt-pagination" @click="toggleMobileMenu">{{ $t('alt_pagination') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/datatables/checkbox" @click="toggleMobileMenu">{{ $t('checkbox') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/datatables/range-search" @click="toggleMobileMenu">{{ $t('range_search') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/datatables/export" @click="toggleMobileMenu">{{ $t('export') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/datatables/sticky-header" @click="toggleMobileMenu">{{ $t('sticky_header') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/datatables/clone-header" @click="toggleMobileMenu">{{ $t('clone_header') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/datatables/column-chooser" @click="toggleMobileMenu">{{ $t('column_chooser') }}</NuxtLink>
                                        </li>
                                    </ul>
                                </vue-collapsible>
                            </li>

                            <li class="menu nav-item">
                                <button
                                    type="button"
                                    class="nav-link group w-full"
                                    :class="{ active: activeDropdown === 'forms' }"
                                    @click="activeDropdown === 'forms' ? (activeDropdown = null) : (activeDropdown = 'forms')"
                                >
                                    <div class="flex items-center">
                                        <icon-menu-forms class="shrink-0 group-hover:!text-primary" />

                                        <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{ $t('forms') }}</span>
                                    </div>
                                    <div :class="{ '-rotate-90 rtl:rotate-90': activeDropdown !== 'forms' }">
                                        <icon-caret-down />
                                    </div>
                                </button>
                                <vue-collapsible :isOpen="activeDropdown === 'forms'">
                                    <ul class="sub-menu text-gray-500">
                                        <li>
                                            <NuxtLink to="/forms/basic" @click="toggleMobileMenu">{{ $t('basic') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/forms/input-group" @click="toggleMobileMenu">{{ $t('input_group') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/forms/layouts" @click="toggleMobileMenu">{{ $t('layouts') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/forms/validation" @click="toggleMobileMenu">{{ $t('validation') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/forms/input-mask" @click="toggleMobileMenu">{{ $t('input_mask') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/forms/select2" @click="toggleMobileMenu">{{ $t('select2') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/forms/touchspin" @click="toggleMobileMenu">{{ $t('touchspin') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/forms/checkbox-radio" @click="toggleMobileMenu">{{ $t('checkbox_and_radio') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/forms/switches" @click="toggleMobileMenu">{{ $t('switches') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/forms/wizards" @click="toggleMobileMenu">{{ $t('wizards') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/forms/file-upload" @click="toggleMobileMenu">{{ $t('file_upload') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/forms/quill-editor" @click="toggleMobileMenu">{{ $t('quill_editor') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/forms/markdown-editor" @click="toggleMobileMenu">{{ $t('markdown_editor') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/forms/date-picker" @click="toggleMobileMenu">{{ $t('date_and_range_picker') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/forms/clipboard" @click="toggleMobileMenu">{{ $t('clipboard') }}</NuxtLink>
                                        </li>
                                    </ul>
                                </vue-collapsible>
                            </li>

                            <h2 class="-mx-4 mb-1 flex items-center bg-white-light/30 px-7 py-3 font-extrabold uppercase dark:bg-dark dark:bg-opacity-[0.08]">
                                <icon-minus class="hidden h-5 w-4 flex-none" />
                                <span>{{ $t('user_and_pages') }}</span>
                            </h2>

                            <li class="menu nav-item">
                                <button
                                    type="button"
                                    class="nav-link group w-full"
                                    :class="{ active: activeDropdown === 'users' }"
                                    @click="activeDropdown === 'users' ? (activeDropdown = null) : (activeDropdown = 'users')"
                                >
                                    <div class="flex items-center">
                                        <icon-menu-users class="shrink-0 group-hover:!text-primary" />

                                        <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{ $t('users') }}</span>
                                    </div>
                                    <div :class="{ '-rotate-90 rtl:rotate-90': activeDropdown !== 'users' }">
                                        <icon-caret-down />
                                    </div>
                                </button>
                                <vue-collapsible :isOpen="activeDropdown === 'users'">
                                    <ul class="sub-menu text-gray-500">
                                        <li>
                                            <NuxtLink to="/users/profile" @click="toggleMobileMenu">{{ $t('profile') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/users/user-account-settings" @click="toggleMobileMenu">{{ $t('account_settings') }}</NuxtLink>
                                        </li>
                                    </ul>
                                </vue-collapsible>
                            </li>

                            <li class="menu nav-item">
                                <button
                                    type="button"
                                    class="nav-link group w-full"
                                    :class="{ active: activeDropdown === 'pages' }"
                                    @click="activeDropdown === 'pages' ? (activeDropdown = null) : (activeDropdown = 'pages')"
                                >
                                    <div class="flex items-center">
                                        <icon-menu-pages class="shrink-0 group-hover:!text-primary" />

                                        <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{ $t('pages') }}</span>
                                    </div>
                                    <div :class="{ '-rotate-90 rtl:rotate-90': activeDropdown !== 'pages' }">
                                        <icon-caret-down />
                                    </div>
                                </button>
                                <vue-collapsible :isOpen="activeDropdown === 'pages'">
                                    <ul class="sub-menu text-gray-500">
                                        <li>
                                            <NuxtLink to="/pages/knowledge-base" @click="toggleMobileMenu">{{ $t('knowledge_base') }}</NuxtLink>
                                        </li>
                                        <li @click="toggleMobileMenu">
                                            <NuxtLink to="/pages/contact-us-boxed" target="_blank">{{ $t('contact_us_boxed') }}</NuxtLink>
                                        </li>
                                        <li @click="toggleMobileMenu">
                                            <NuxtLink to="/pages/contact-us-cover" target="_blank">{{ $t('contact_us_cover') }}</NuxtLink>
                                        </li>
                                        <li>
                                            <NuxtLink to="/pages/faq" @click="toggleMobileMenu">{{ $t('faq') }}</NuxtLink>
                                        </li>
                                        <li @click="toggleMobileMenu">
                                            <NuxtLink to="/pages/coming-soon-boxed" target="_blank">{{ $t('coming_soon_boxed') }}</NuxtLink>
                                        </li>
                                        <li @click="toggleMobileMenu">
                                            <NuxtLink to="/pages/coming-soon-cover" target="_blank">{{ $t('coming_soon_cover') }}</NuxtLink>
                                        </li>
                                        <li class="menu nav-item">
                                            <button
                                                type="button"
                                                class="w-full before:h-[5px] before:w-[5px] before:rounded before:bg-gray-300 hover:bg-gray-100 ltr:before:mr-2 rtl:before:ml-2 dark:text-[#888ea8] dark:hover:bg-gray-900"
                                                @click="subActive === 'error' ? (subActive = null) : (subActive = 'error')"
                                            >
                                                {{ $t('error') }}
                                                <div class="ltr:ml-auto rtl:mr-auto" :class="{ '-rotate-90 rtl:rotate-90': subActive !== 'error' }">
                                                    <icon-carets-down :fill="true" class="h-4 w-4" />
                                                </div>
                                            </button>

                                            <vue-collapsible :isOpen="subActive === 'error'">
                                                <ul :unmount="false" class="sub-menu text-gray-500">
                                                    <li @click="toggleMobileMenu">
                                                        <NuxtLink to="/pages/error404" target="_blank">{{ $t('404') }}</NuxtLink>
                                                    </li>
                                                    <li @click="toggleMobileMenu">
                                                        <NuxtLink to="/pages/error500" target="_blank">{{ $t('500') }}</NuxtLink>
                                                    </li>
                                                    <li @click="toggleMobileMenu">
                                                        <NuxtLink to="/pages/error503" target="_blank">{{ $t('503') }}</NuxtLink>
                                                    </li>
                                                </ul>
                                            </vue-collapsible>
                                        </li>
                                        <li>
                                            <NuxtLink to="/pages/maintenence" target="_blank">{{ $t('maintenence') }}</NuxtLink>
                                        </li>
                                    </ul>
                                </vue-collapsible>
                            </li>

                            <li class="menu nav-item">
                                <button
                                    type="button"
                                    class="nav-link group w-full"
                                    :class="{ active: activeDropdown === 'authentication' }"
                                    @click="activeDropdown === 'authentication' ? (activeDropdown = null) : (activeDropdown = 'authentication')"
                                >
                                    <div class="flex items-center">
                                        <icon-menu-authentication class="shrink-0 group-hover:!text-primary" />

                                        <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{
                                            $t('authentication')
                                        }}</span>
                                    </div>
                                    <div :class="{ '-rotate-90 rtl:rotate-90': activeDropdown !== 'authentication' }">
                                        <icon-caret-down />
                                    </div>
                                </button>
                                <vue-collapsible :isOpen="activeDropdown === 'authentication'">
                                    <ul class="sub-menu text-gray-500">
                                        <li @click="toggleMobileMenu">
                                            <NuxtLink to="/auth/boxed-signin" target="_blank">{{ $t('login_boxed') }}</NuxtLink>
                                        </li>
                                        <li @click="toggleMobileMenu">
                                            <NuxtLink to="/auth/boxed-signup" target="_blank">{{ $t('register_boxed') }}</NuxtLink>
                                        </li>
                                        <li @click="toggleMobileMenu">
                                            <NuxtLink to="/auth/boxed-lockscreen" target="_blank">{{ $t('unlock_boxed') }}</NuxtLink>
                                        </li>
                                        <li @click="toggleMobileMenu">
                                            <NuxtLink to="/auth/boxed-password-reset" target="_blank">{{ $t('recover_id_boxed') }}</NuxtLink>
                                        </li>
                                        <li @click="toggleMobileMenu">
                                            <NuxtLink to="/auth/cover-login" target="_blank">{{ $t('login_cover') }}</NuxtLink>
                                        </li>
                                        <li @click="toggleMobileMenu">
                                            <NuxtLink to="/auth/cover-signup" target="_blank">{{ $t('register_cover') }}</NuxtLink>
                                        </li>
                                        <li @click="toggleMobileMenu">
                                            <NuxtLink to="/auth/cover-lockscreen" target="_blank">{{ $t('unlock_cover') }}</NuxtLink>
                                        </li>
                                        <li @click="toggleMobileMenu">
                                            <NuxtLink to="/auth/cover-password-reset" target="_blank">{{ $t('recover_id_cover') }}</NuxtLink>
                                        </li>
                                    </ul>
                                </vue-collapsible>
                            </li>

                            <h2 class="-mx-4 mb-1 flex items-center bg-white-light/30 px-7 py-3 font-extrabold uppercase dark:bg-dark dark:bg-opacity-[0.08]">
                                <icon-minus class="hidden h-5 w-4 flex-none" />
                                <span>{{ $t('supports') }}</span>
                            </h2>

                            <li class="menu nav-item">
                                <a href="https://vristo.sbthemes.com" target="_blank" class="nav-link group">
                                    <div class="flex items-center">
                                        <icon-menu-documentation class="shrink-0 group-hover:!text-primary" />

                                        <span class="text-black ltr:pl-3 rtl:pr-3 dark:text-[#506690] dark:group-hover:text-white-dark">{{
                                            $t('documentation')
                                        }}</span>
                                    </div>
                                </a>
                            </li> -->
                        </ul>
                    </perfect-scrollbar>
                </client-only>
            </div>
        </nav>
    </div>
</template>

<script lang="ts" setup>
    import { ref, onMounted } from 'vue';

    import { useAppStore } from '@/stores/index';
    import VueCollapsible from 'vue-height-collapsible/vue3';
    const store = useAppStore();
    const activeDropdown: any = ref('');
    const subActive: any = ref('');

    onMounted(() => {
        setTimeout(() => {
            const selector = document.querySelector('.sidebar ul a[href="' + window.location.pathname + '"]');

            if (selector) {
                selector.classList.add('active');
                const ul: any = selector.closest('ul.sub-menu');
                if (ul) {
                    let ele: any = ul.closest('li.menu').querySelectorAll('.nav-link') || [];

                    if (ele.length) {
                        ele = ele[0];
                        setTimeout(() => {
                            ele.click();
                        });
                    }
                }
            }
        });
    });

    const toggleMobileMenu = () => {
        if (window.innerWidth < 1024) {
            store.toggleSidebar();
        }
    };
</script>
