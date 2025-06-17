<template>
    <div>
        <ul class="flex space-x-2 rtl:space-x-reverse">
            <li>
                <a href="javascript:;" class="text-primary hover:underline">Dashboard</a>
            </li>
            <li class="before:content-['/'] ltr:before:mr-2 rtl:before:ml-2">
                <span>Finance</span>
            </li>
        </ul>
        <div class="pt-5">
            <div class="mb-6 grid grid-cols-1 gap-6 text-white sm:grid-cols-2 xl:grid-cols-4">
                <!-- Users Visit -->
                <div class="panel bg-gradient-to-r from-cyan-500 to-cyan-400">
                    <div class="flex justify-between">
                        <div class="text-md font-semibold ltr:mr-1 rtl:ml-1">Users Visit</div>
                        <div class="dropdown">
                            <client-only>
                                <Popper :placement="store.rtlClass === 'rtl' ? 'bottom-start' : 'bottom-end'" offsetDistance="0" class="align-middle">
                                    <button type="button">
                                        <icon-horizontal-dots class="opacity-70 hover:opacity-80" />
                                    </button>
                                    <template #content="{ close }">
                                        <ul @click="close()" class="text-black dark:text-white-dark">
                                            <li>
                                                <a href="javascript:;">View Report</a>
                                            </li>
                                            <li>
                                                <a href="javascript:;">Edit Report</a>
                                            </li>
                                        </ul>
                                    </template>
                                </Popper>
                            </client-only>
                        </div>
                    </div>
                    <div class="mt-5 flex items-center">
                        <div class="text-3xl font-bold ltr:mr-3 rtl:ml-3">$170.46</div>
                        <div class="badge bg-white/30">+ 2.35%</div>
                    </div>
                    <div class="mt-5 flex items-center font-semibold">
                        <icon-eye class="shrink-0 ltr:mr-2 rtl:ml-2" />
                        Last Week 44,700
                    </div>
                </div>

                <!-- Sessions -->
                <div class="panel bg-gradient-to-r from-violet-500 to-violet-400">
                    <div class="flex justify-between">
                        <div class="text-md font-semibold ltr:mr-1 rtl:ml-1">Sessions</div>
                        <div class="dropdown">
                            <client-only>
                                <Popper :placement="store.rtlClass === 'rtl' ? 'bottom-start' : 'bottom-end'" offsetDistance="0" class="align-middle">
                                    <button type="button">
                                        <icon-horizontal-dots class="opacity-70 hover:opacity-80" />
                                    </button>
                                    <template #content="{ close }">
                                        <ul @click="close()" class="text-black dark:text-white-dark">
                                            <li>
                                                <a href="javascript:;">View Report</a>
                                            </li>
                                            <li>
                                                <a href="javascript:;">Edit Report</a>
                                            </li>
                                        </ul>
                                    </template>
                                </Popper>
                            </client-only>
                        </div>
                    </div>
                    <div class="mt-5 flex items-center">
                        <div class="text-3xl font-bold ltr:mr-3 rtl:ml-3">74,137</div>
                        <div class="badge bg-white/30">- 2.35%</div>
                    </div>
                    <div class="mt-5 flex items-center font-semibold">
                        <icon-eye class="shrink-0 ltr:mr-2 rtl:ml-2" />
                        Last Week 84,709
                    </div>
                </div>

                <!-- Time On-Site -->
                <div class="panel bg-gradient-to-r from-blue-500 to-blue-400">
                    <div class="flex justify-between">
                        <div class="text-md font-semibold ltr:mr-1 rtl:ml-1">Time On-Site</div>
                        <div class="dropdown">
                            <client-only>
                                <Popper :placement="store.rtlClass === 'rtl' ? 'bottom-start' : 'bottom-end'" offsetDistance="0" class="align-middle">
                                    <button type="button">
                                        <icon-horizontal-dots class="opacity-70 hover:opacity-80" />
                                    </button>
                                    <template #content="{ close }">
                                        <ul @click="close()" class="text-black dark:text-white-dark">
                                            <li>
                                                <a href="javascript:;">View Report</a>
                                            </li>
                                            <li>
                                                <a href="javascript:;">Edit Report</a>
                                            </li>
                                        </ul>
                                    </template>
                                </Popper>
                            </client-only>
                        </div>
                    </div>
                    <div class="mt-5 flex items-center">
                        <div class="text-3xl font-bold ltr:mr-3 rtl:ml-3">38,085</div>
                        <div class="badge bg-white/30">+ 1.35%</div>
                    </div>
                    <div class="mt-5 flex items-center font-semibold">
                        <icon-eye class="shrink-0 ltr:mr-2 rtl:ml-2" />
                        Last Week 37,894
                    </div>
                </div>

                <!-- Bounce Rate -->
                <div class="panel bg-gradient-to-r from-fuchsia-500 to-fuchsia-400">
                    <div class="flex justify-between">
                        <div class="text-md font-semibold ltr:mr-1 rtl:ml-1">Bounce Rate</div>
                        <div class="dropdown">
                            <client-only>
                                <Popper :placement="store.rtlClass === 'rtl' ? 'bottom-start' : 'bottom-end'" offsetDistance="0" class="align-middle">
                                    <button type="button">
                                        <icon-horizontal-dots class="opacity-70 hover:opacity-80" />
                                    </button>
                                    <template #content="{ close }">
                                        <ul @click="close()" class="text-black dark:text-white-dark">
                                            <li>
                                                <a href="javascript:;">View Report</a>
                                            </li>
                                            <li>
                                                <a href="javascript:;">Edit Report</a>
                                            </li>
                                        </ul>
                                    </template>
                                </Popper>
                            </client-only>
                        </div>
                    </div>
                    <div class="mt-5 flex items-center">
                        <div class="text-3xl font-bold ltr:mr-3 rtl:ml-3">49.10%</div>
                        <div class="badge bg-white/30">- 0.35%</div>
                    </div>
                    <div class="mt-5 flex items-center font-semibold">
                        <icon-eye class="shrink-0 ltr:mr-2 rtl:ml-2" />
                        Last Week 50.01%
                    </div>
                </div>
            </div>

            <div class="grid grid-cols-1 gap-6 xl:grid-cols-2">
                <!-- Favorites -->
                <div>
                    <div class="mb-5 flex items-center font-bold">
                        <span class="text-lg">Favorites</span>
                        <a href="javascript:;" class="text-primary hover:text-black ltr:ml-auto rtl:mr-auto dark:hover:text-white-dark"> See All </a>
                    </div>
                    <div class="grid grid-cols-1 gap-6 sm:grid-cols-3 md:mb-5">
                        <!-- Bitcoin -->
                        <div class="panel">
                            <div class="mb-5 flex items-center font-semibold">
                                <div class="grid h-10 w-10 shrink-0 place-content-center rounded-full">
                                    <icon-bitcoin />
                                </div>
                                <div class="ltr:ml-2 rtl:mr-2">
                                    <h6 class="text-dark dark:text-white-light">BTC</h6>
                                    <p class="text-xs text-white-dark">Bitcoin</p>
                                </div>
                            </div>
                            <div class="mb-5 overflow-hidden">
                                <apexchart height="45" :options="bitcoin" :series="bitcoinSeries">
                                    <!-- loader -->
                                    <div class="grid min-h-[45px] place-content-center bg-white-light/30 dark:bg-dark dark:bg-opacity-[0.08]">
                                        <span
                                            class="inline-flex h-5 w-5 animate-spin rounded-full border-2 border-black !border-l-transparent dark:border-white"
                                        ></span>
                                    </div>
                                </apexchart>
                            </div>
                            <div class="flex items-center justify-between text-base font-bold">
                                $20,000 <span class="text-sm font-normal text-success">+0.25%</span>
                            </div>
                        </div>

                        <!-- Ethereum -->
                        <div class="panel">
                            <div class="mb-5 flex items-center font-semibold">
                                <div class="grid h-10 w-10 shrink-0 place-content-center rounded-full bg-warning p-2">
                                    <icon-ethereum />
                                </div>
                                <div class="ltr:ml-2 rtl:mr-2">
                                    <h6 class="text-dark dark:text-white-light">ETH</h6>
                                    <p class="text-xs text-white-dark">Ethereum</p>
                                </div>
                            </div>
                            <div class="mb-5 overflow-hidden">
                                <apexchart height="45" :options="ethereum" :series="ethereumSeries">
                                    <!-- loader -->
                                    <div class="grid min-h-[45px] place-content-center bg-white-light/30 dark:bg-dark dark:bg-opacity-[0.08]">
                                        <span
                                            class="inline-flex h-5 w-5 animate-spin rounded-full border-2 border-black !border-l-transparent dark:border-white"
                                        ></span>
                                    </div>
                                </apexchart>
                            </div>
                            <div class="flex items-center justify-between text-base font-bold">
                                $21,000 <span class="text-sm font-normal text-danger">-1.25%</span>
                            </div>
                        </div>

                        <!-- Litecoin -->
                        <div class="panel">
                            <div class="mb-5 flex items-center font-semibold">
                                <div class="grid h-10 w-10 shrink-0 place-content-center rounded-full">
                                    <icon-litecoin />
                                </div>
                                <div class="ltr:ml-2 rtl:mr-2">
                                    <h6 class="text-dark dark:text-white-light">LTC</h6>
                                    <p class="text-xs text-white-dark">Litecoin</p>
                                </div>
                            </div>
                            <div class="mb-5 overflow-hidden">
                                <apexchart height="45" :options="litecoin" :series="litecoinSeries">
                                    <!-- loader -->
                                    <div class="grid min-h-[45px] place-content-center bg-white-light/30 dark:bg-dark dark:bg-opacity-[0.08]">
                                        <span
                                            class="inline-flex h-5 w-5 animate-spin rounded-full border-2 border-black !border-l-transparent dark:border-white"
                                        ></span>
                                    </div>
                                </apexchart>
                            </div>
                            <div class="flex items-center justify-between text-base font-bold">
                                $11,657 <span class="text-sm font-normal text-success">+0.25%</span>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Live Prices -->
                <div>
                    <div class="mb-5 flex items-center font-bold">
                        <span class="text-lg">Live Prices</span>
                        <a href="javascript:;" class="text-primary hover:text-black ltr:ml-auto rtl:mr-auto dark:hover:text-white-dark"> See All </a>
                    </div>
                    <div class="mb-6 grid grid-cols-1 gap-6 sm:grid-cols-3">
                        <!-- Binance -->
                        <div class="panel">
                            <div class="mb-5 flex items-center font-semibold">
                                <div class="grid h-10 w-10 shrink-0 place-content-center rounded-full">
                                    <icon-binance />
                                </div>
                                <div class="ltr:ml-2 rtl:mr-2">
                                    <h6 class="text-dark dark:text-white-light">BNB</h6>
                                    <p class="text-xs text-white-dark">Binance</p>
                                </div>
                            </div>
                            <div class="mb-5 overflow-hidden">
                                <apexchart height="45" :options="binance" :series="binanceSeries">
                                    <!-- loader -->
                                    <div class="grid min-h-[45px] place-content-center bg-white-light/30 dark:bg-dark dark:bg-opacity-[0.08]">
                                        <span
                                            class="inline-flex h-5 w-5 animate-spin rounded-full border-2 border-black !border-l-transparent dark:border-white"
                                        ></span>
                                    </div>
                                </apexchart>
                            </div>
                            <div class="flex items-center justify-between text-base font-bold">
                                $21,000 <span class="text-sm font-normal text-danger">-1.25%</span>
                            </div>
                        </div>

                        <!-- Tether -->
                        <div class="panel">
                            <div class="mb-5 flex items-center font-semibold">
                                <div class="grid h-10 w-10 shrink-0 place-content-center rounded-full">
                                    <icon-tether />
                                </div>
                                <div class="ltr:ml-2 rtl:mr-2">
                                    <h6 class="text-dark dark:text-white-light">USDT</h6>
                                    <p class="text-xs text-white-dark">Tether</p>
                                </div>
                            </div>
                            <div class="mb-5 overflow-hidden">
                                <apexchart height="45" :options="tether" :series="tetherSeries">
                                    <!-- loader -->
                                    <div class="grid min-h-[45px] place-content-center bg-white-light/30 dark:bg-dark dark:bg-opacity-[0.08]">
                                        <span
                                            class="inline-flex h-5 w-5 animate-spin rounded-full border-2 border-black !border-l-transparent dark:border-white"
                                        ></span>
                                    </div>
                                </apexchart>
                            </div>
                            <div class="flex items-center justify-between text-base font-bold">
                                $20,000 <span class="text-sm font-normal text-success">+0.25%</span>
                            </div>
                        </div>

                        <!-- Solana -->
                        <div class="panel">
                            <div class="mb-5 flex items-center font-semibold">
                                <div class="grid h-10 w-10 shrink-0 place-content-center rounded-full bg-warning p-2">
                                    <icon-solana />
                                </div>
                                <div class="ltr:ml-2 rtl:mr-2">
                                    <h6 class="text-dark dark:text-white-light">SOL</h6>
                                    <p class="text-xs text-white-dark">Solana</p>
                                </div>
                            </div>
                            <div class="mb-5 overflow-hidden">
                                <apexchart height="45" :options="solana" :series="solanaSeries">
                                    <!-- loader -->
                                    <div class="grid min-h-[45px] place-content-center bg-white-light/30 dark:bg-dark dark:bg-opacity-[0.08]">
                                        <span
                                            class="inline-flex h-5 w-5 animate-spin rounded-full border-2 border-black !border-l-transparent dark:border-white"
                                        ></span>
                                    </div>
                                </apexchart>
                            </div>
                            <div class="flex items-center justify-between text-base font-bold">
                                $21,000 <span class="text-sm font-normal text-danger">-1.25%</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="grid grid-cols-1 gap-6 xl:grid-cols-2">
                <div class="grid gap-6 xl:grid-flow-row">
                    <!-- Previous Statement -->
                    <div class="panel overflow-hidden">
                        <div class="flex items-center justify-between">
                            <div>
                                <div class="text-lg font-bold">Previous Statement</div>
                                <div class="text-success">Paid on June 27, 2022</div>
                            </div>
                            <div class="dropdown">
                                <client-only>
                                    <Popper :placement="store.rtlClass === 'rtl' ? 'bottom-start' : 'bottom-end'" offsetDistance="0" class="align-middle">
                                        <button type="button">
                                            <icon-horizontal-dots class="opacity-70 hover:opacity-80" />
                                        </button>
                                        <template #content="{ close }">
                                            <ul @click="close()">
                                                <li>
                                                    <a href="javascript:;">View Report</a>
                                                </li>
                                                <li>
                                                    <a href="javascript:;">Edit Report</a>
                                                </li>
                                            </ul>
                                        </template>
                                    </Popper>
                                </client-only>
                            </div>
                        </div>
                        <div class="relative mt-10">
                            <div class="absolute -bottom-12 h-24 w-24 ltr:-right-12 rtl:-left-12">
                                <icon-circle-check class="h-full w-full text-success opacity-20" />
                            </div>
                            <div class="grid grid-cols-2 gap-6 md:grid-cols-3">
                                <div>
                                    <div class="text-primary">Card Limit</div>
                                    <div class="mt-2 text-2xl font-semibold">$50,000.00</div>
                                </div>
                                <div>
                                    <div class="text-primary">Spent</div>
                                    <div class="mt-2 text-2xl font-semibold">$15,000.00</div>
                                </div>
                                <div>
                                    <div class="text-primary">Minimum</div>
                                    <div class="mt-2 text-2xl font-semibold">$2,500.00</div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Current Statement -->
                    <div class="panel overflow-hidden">
                        <div class="flex items-center justify-between">
                            <div>
                                <div class="text-lg font-bold">Current Statement</div>
                                <div class="text-danger">Must be paid before July 27, 2022</div>
                            </div>
                            <div class="dropdown">
                                <client-only>
                                    <Popper :placement="store.rtlClass === 'rtl' ? 'bottom-start' : 'bottom-end'" offsetDistance="0" class="align-middle">
                                        <button type="button">
                                            <icon-horizontal-dots class="opacity-70 hover:opacity-80" />
                                        </button>
                                        <template #content="{ close }">
                                            <ul @click="close()">
                                                <li>
                                                    <a href="javascript:;">View Report</a>
                                                </li>
                                                <li>
                                                    <a href="javascript:;">Edit Report</a>
                                                </li>
                                            </ul>
                                        </template>
                                    </Popper>
                                </client-only>
                            </div>
                        </div>
                        <div class="relative mt-10">
                            <div class="absolute -bottom-12 h-24 w-24 ltr:-right-12 rtl:-left-12">
                                <icon-info-circle class="h-full w-24 text-danger opacity-20" />
                            </div>

                            <div class="grid grid-cols-2 gap-6 md:grid-cols-3">
                                <div>
                                    <div class="text-primary">Card Limit</div>
                                    <div class="mt-2 text-2xl font-semibold">$50,000.00</div>
                                </div>
                                <div>
                                    <div class="text-primary">Spent</div>
                                    <div class="mt-2 text-2xl font-semibold">$30,500.00</div>
                                </div>
                                <div>
                                    <div class="text-primary">Minimum</div>
                                    <div class="mt-2 text-2xl font-semibold">$8,000.00</div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Recent Transactions -->
                <div class="panel">
                    <div class="mb-5 text-lg font-bold">Recent Transactions</div>
                    <div class="table-responsive">
                        <table>
                            <thead>
                                <tr>
                                    <th class="ltr:rounded-l-md rtl:rounded-r-md">ID</th>
                                    <th>DATE</th>
                                    <th>NAME</th>
                                    <th>AMOUNT</th>
                                    <th class="text-center ltr:rounded-r-md rtl:rounded-l-md">STATUS</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td class="font-semibold">#01</td>
                                    <td class="whitespace-nowrap">Oct 08, 2021</td>
                                    <td class="whitespace-nowrap">Eric Page</td>
                                    <td>$1,358.75</td>
                                    <td class="text-center">
                                        <span class="badge rounded-full bg-success/20 text-success hover:top-0">Completed</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="font-semibold">#02</td>
                                    <td class="whitespace-nowrap">Dec 18, 2021</td>
                                    <td class="whitespace-nowrap">Nita Parr</td>
                                    <td>-$1,042.82</td>
                                    <td class="text-center">
                                        <span class="badge rounded-full bg-info/20 text-info hover:top-0">In Process</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="font-semibold">#03</td>
                                    <td class="whitespace-nowrap">Dec 25, 2021</td>
                                    <td class="whitespace-nowrap">Carl Bell</td>
                                    <td>$1,828.16</td>
                                    <td class="text-center">
                                        <span class="badge rounded-full bg-danger/20 text-danger hover:top-0">Pending</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="font-semibold">#04</td>
                                    <td class="whitespace-nowrap">Nov 29, 2021</td>
                                    <td class="whitespace-nowrap">Dan Hart</td>
                                    <td>$1,647.55</td>
                                    <td class="text-center">
                                        <span class="badge rounded-full bg-success/20 text-success hover:top-0">Completed</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="font-semibold">#05</td>
                                    <td class="whitespace-nowrap">Nov 24, 2021</td>
                                    <td class="whitespace-nowrap">Jake Ross</td>
                                    <td>$927.43</td>
                                    <td class="text-center">
                                        <span class="badge rounded-full bg-success/20 text-success hover:top-0">Completed</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="font-semibold">#06</td>
                                    <td class="whitespace-nowrap">Jan 26, 2022</td>
                                    <td class="whitespace-nowrap">Anna Bell</td>
                                    <td>$250.00</td>
                                    <td class="text-center">
                                        <span class="badge rounded-full bg-info/20 text-info hover:top-0">In Process</span>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
<script lang="ts" setup>
    import { ref, computed, onMounted } from 'vue';
    import { useAppStore } from '@/stores/index';
    const store = useAppStore();
    useHead({ title: 'Finance Admin' });

    onMounted(() => {
        nextTick(() => {
            window.dispatchEvent(new Event('resize'));
        });
    });

    // bitcoin
    const bitcoin = computed(() => {
        return {
            chart: {
                height: 45,
                type: 'line',
                sparkline: {
                    enabled: true,
                },
            },
            stroke: {
                width: 2,
            },
            markers: {
                size: 0,
            },
            colors: ['#00ab55'],
            grid: {
                padding: {
                    top: 0,
                    bottom: 0,
                    left: 0,
                },
            },
            tooltip: {
                x: {
                    show: false,
                },
                y: {
                    title: {
                        formatter: (val: any) => {
                            return '';
                        },
                    },
                },
            },
            responsive: [
                {
                    breakPoint: 576,
                    options: {
                        chart: {
                            height: 95,
                        },
                        grid: {
                            padding: {
                                top: 45,
                                bottom: 0,
                                left: 0,
                            },
                        },
                    },
                },
            ],
        };
    });

    const bitcoinSeries = ref([
        {
            data: [21, 9, 36, 12, 44, 25, 59, 41, 25, 66],
        },
    ]);

    // ethereum
    const ethereum = computed(() => {
        return {
            chart: {
                height: 45,
                type: 'line',
                sparkline: {
                    enabled: true,
                },
            },
            stroke: {
                width: 2,
            },
            markers: {
                size: 0,
            },
            colors: ['#e7515a'],
            grid: {
                padding: {
                    top: 0,
                    bottom: 0,
                    left: 0,
                },
            },
            tooltip: {
                x: {
                    show: false,
                },
                y: {
                    title: {
                        formatter: (val: any) => {
                            return '';
                        },
                    },
                },
            },
            responsive: [
                {
                    breakPoint: 576,
                    options: {
                        chart: {
                            height: 95,
                        },
                        grid: {
                            padding: {
                                top: 45,
                                bottom: 0,
                                left: 0,
                            },
                        },
                    },
                },
            ],
        };
    });

    const ethereumSeries = ref([
        {
            data: [44, 25, 59, 41, 66, 25, 21, 9, 36, 12],
        },
    ]);

    // litecoin
    const litecoin = computed(() => {
        return {
            chart: {
                height: 45,
                type: 'line',
                sparkline: {
                    enabled: true,
                },
            },
            stroke: {
                width: 2,
            },
            markers: {
                size: 0,
            },
            colors: ['#00ab55'],
            grid: {
                padding: {
                    top: 0,
                    bottom: 0,
                    left: 0,
                },
            },
            tooltip: {
                x: {
                    show: false,
                },
                y: {
                    title: {
                        formatter: (val: any) => {
                            return '';
                        },
                    },
                },
            },
            responsive: [
                {
                    breakPoint: 576,
                    options: {
                        chart: {
                            height: 95,
                        },
                        grid: {
                            padding: {
                                top: 45,
                                bottom: 0,
                                left: 0,
                            },
                        },
                    },
                },
            ],
        };
    });

    const litecoinSeries = ref([
        {
            data: [9, 21, 36, 12, 66, 25, 44, 25, 41, 59],
        },
    ]);

    // binance
    const binance = computed(() => {
        return {
            chart: {
                height: 45,
                type: 'line',
                sparkline: {
                    enabled: true,
                },
            },
            stroke: {
                width: 2,
            },
            markers: {
                size: 0,
            },
            colors: ['#e7515a'],
            grid: {
                padding: {
                    top: 0,
                    bottom: 0,
                    left: 0,
                },
            },
            tooltip: {
                x: {
                    show: false,
                },
                y: {
                    title: {
                        formatter: (val: any) => {
                            return '';
                        },
                    },
                },
            },
            responsive: [
                {
                    breakPoint: 576,
                    options: {
                        chart: {
                            height: 95,
                        },
                        grid: {
                            padding: {
                                top: 45,
                                bottom: 0,
                                left: 0,
                            },
                        },
                    },
                },
            ],
        };
    });

    const binanceSeries = ref([
        {
            data: [25, 44, 25, 59, 41, 21, 36, 12, 19, 9],
        },
    ]);

    // tether
    const tether = computed(() => {
        return {
            chart: {
                height: 45,
                type: 'line',
                sparkline: {
                    enabled: true,
                },
            },
            stroke: {
                width: 2,
            },
            markers: {
                size: 0,
            },
            colors: ['#00ab55'],
            grid: {
                padding: {
                    top: 0,
                    bottom: 0,
                    left: 0,
                },
            },
            tooltip: {
                x: {
                    show: false,
                },
                y: {
                    title: {
                        formatter: (val: any) => {
                            return '';
                        },
                    },
                },
            },
            responsive: [
                {
                    breakPoint: 576,
                    options: {
                        chart: {
                            height: 95,
                        },
                        grid: {
                            padding: {
                                top: 45,
                                bottom: 0,
                                left: 0,
                            },
                        },
                    },
                },
            ],
        };
    });

    const tetherSeries = ref([
        {
            data: [21, 59, 41, 44, 25, 66, 9, 36, 25, 12],
        },
    ]);

    // solana
    const solana = computed(() => {
        return {
            chart: {
                height: 45,
                type: 'line',
                sparkline: {
                    enabled: true,
                },
            },
            stroke: {
                width: 2,
            },
            markers: {
                size: 0,
            },
            colors: ['#e7515a'],
            grid: {
                padding: {
                    top: 0,
                    bottom: 0,
                    left: 0,
                },
            },
            tooltip: {
                x: {
                    show: false,
                },
                y: {
                    title: {
                        formatter: (val: any) => {
                            return '';
                        },
                    },
                },
            },
            responsive: [
                {
                    breakPoint: 576,
                    options: {
                        chart: {
                            height: 95,
                        },
                        grid: {
                            padding: {
                                top: 45,
                                bottom: 0,
                                left: 0,
                            },
                        },
                    },
                },
            ],
        };
    });

    const solanaSeries = ref([
        {
            data: [21, -9, 36, -12, 44, 25, 59, -41, 66, -25],
        },
    ]);
</script>
