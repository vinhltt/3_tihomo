<template>
    <div>
        <div class="panel">
            <div class="mb-5">
                <div class="mb-4 flex flex-col items-center justify-center sm:flex-row sm:justify-between">
                    <div class="mb-4 sm:mb-0">
                        <div class="text-center text-lg font-semibold ltr:sm:text-left rtl:sm:text-right">Calendar</div>
                        <div class="mt-2 flex flex-wrap items-center justify-center sm:justify-start">
                            <div class="flex items-center ltr:mr-4 rtl:ml-4">
                                <div class="h-2.5 w-2.5 rounded-sm bg-primary ltr:mr-2 rtl:ml-2"></div>
                                <div>Work</div>
                            </div>
                            <div class="flex items-center ltr:mr-4 rtl:ml-4">
                                <div class="h-2.5 w-2.5 rounded-sm bg-info ltr:mr-2 rtl:ml-2"></div>
                                <div>Travel</div>
                            </div>
                            <div class="flex items-center ltr:mr-4 rtl:ml-4">
                                <div class="h-2.5 w-2.5 rounded-sm bg-success ltr:mr-2 rtl:ml-2"></div>
                                <div>Personal</div>
                            </div>
                            <div class="flex items-center">
                                <div class="h-2.5 w-2.5 rounded-sm bg-danger ltr:mr-2 rtl:ml-2"></div>
                                <div>Important</div>
                            </div>
                        </div>
                    </div>
                    <button type="button" class="btn btn-primary" @click="editEvent()">
                        <icon-plus class="ltr:mr-2 rtl:ml-2" />
                        Create Event
                    </button>
                </div>
                <div class="calendar-wrapper">
                    <client-only>
                        <FullCalendar ref="calendar" :options="calendarOptions">
                            <template v-slot:eventContent="arg">
                                <div class="fc-event-main-frame flex items-center px-1 py-0.5 text-white">
                                    <div class="fc-event-time px-0.5 font-semibold">
                                        {{ arg.timeText }}
                                    </div>
                                    <div class="fc-event-title-container">
                                        <div class="fc-event-title fc-sticky px-0.5 !font-medium">
                                            {{ arg.event.title }}
                                        </div>
                                    </div>
                                </div>
                            </template>
                        </FullCalendar>
                    </client-only>
                </div>
            </div>
        </div>
        <!-- add event modal -->
        <client-only>
            <TransitionRoot appear :show="isAddEventModal" as="template">
                <Dialog as="div" @close="isAddEventModal = false" class="relative z-[51]">
                    <TransitionChild
                        as="template"
                        enter="duration-300 ease-out"
                        enter-from="opacity-0"
                        enter-to="opacity-100"
                        leave="duration-200 ease-in"
                        leave-from="opacity-100"
                        leave-to="opacity-0"
                    >
                        <DialogOverlay class="fixed inset-0 bg-[black]/60" />
                    </TransitionChild>

                    <div class="fixed inset-0 overflow-y-auto">
                        <div class="flex min-h-full items-center justify-center px-4 py-8">
                            <TransitionChild
                                as="template"
                                enter="duration-300 ease-out"
                                enter-from="opacity-0 scale-95"
                                enter-to="opacity-100 scale-100"
                                leave="duration-200 ease-in"
                                leave-from="opacity-100 scale-100"
                                leave-to="opacity-0 scale-95"
                            >
                                <DialogPanel class="panel w-full max-w-lg overflow-hidden rounded-lg border-0 p-0 text-black dark:text-white-dark">
                                    <button
                                        type="button"
                                        class="absolute top-4 text-gray-400 outline-none hover:text-gray-800 ltr:right-4 rtl:left-4 dark:hover:text-gray-600"
                                        @click="isAddEventModal = false"
                                    >
                                        <icon-x />
                                    </button>
                                    <div class="bg-[#fbfbfb] py-3 text-lg font-medium ltr:pl-5 ltr:pr-[50px] rtl:pr-5 rtl:pl-[50px] dark:bg-[#121c2c]">
                                        {{ params.id ? 'Edit Event' : 'Add Event' }}
                                    </div>
                                    <div class="p-5">
                                        <form @submit.prevent="saveEvent">
                                            <div class="mb-5">
                                                <label for="title">Event Title :</label>
                                                <input
                                                    id="title"
                                                    type="text"
                                                    name="title"
                                                    class="form-input"
                                                    placeholder="Enter Event Title"
                                                    v-model="params.title"
                                                    required
                                                />
                                                <div class="mt-2 text-danger" id="titleErr"></div>
                                            </div>

                                            <div class="mb-5">
                                                <label for="dateStart">From :</label>
                                                <input
                                                    id="dateStart"
                                                    type="datetime-local"
                                                    name="start"
                                                    class="form-input"
                                                    placeholder="Event Start Date"
                                                    v-model="params.start"
                                                    :min="minStartDate"
                                                    @change="startDateChange($event)"
                                                    required
                                                />
                                                <div class="mt-2 text-danger" id="startDateErr"></div>
                                            </div>
                                            <div class="mb-5">
                                                <label for="dateEnd">To :</label>
                                                <input
                                                    id="dateEnd"
                                                    type="datetime-local"
                                                    name="end"
                                                    class="form-input"
                                                    placeholder="Event End Date"
                                                    v-model="params.end"
                                                    :min="minEndDate"
                                                    required
                                                />
                                                <div class="mt-2 text-danger" id="endDateErr"></div>
                                            </div>
                                            <div class="mb-5">
                                                <label for="description">Event Description :</label>
                                                <textarea
                                                    id="description"
                                                    name="description"
                                                    class="form-textarea min-h-[130px]"
                                                    placeholder="Enter Event Description"
                                                    v-model="params.description"
                                                ></textarea>
                                            </div>
                                            <div class="mb-5">
                                                <label>Badge:</label>
                                                <div class="mt-3">
                                                    <label class="inline-flex cursor-pointer ltr:mr-3 rtl:ml-3">
                                                        <input type="radio" class="form-radio" name="badge" value="primary" v-model="params.type" />
                                                        <span class="ltr:pl-2 rtl:pr-2">Work</span>
                                                    </label>
                                                    <label class="inline-flex cursor-pointer ltr:mr-3 rtl:ml-3">
                                                        <input type="radio" class="form-radio text-info" name="badge" value="info" v-model="params.type" />
                                                        <span class="ltr:pl-2 rtl:pr-2">Travel</span>
                                                    </label>
                                                    <label class="inline-flex cursor-pointer ltr:mr-3 rtl:ml-3">
                                                        <input
                                                            type="radio"
                                                            class="form-radio text-success"
                                                            name="badge"
                                                            value="success"
                                                            v-model="params.type"
                                                        />
                                                        <span class="ltr:pl-2 rtl:pr-2">Personal</span>
                                                    </label>
                                                    <label class="inline-flex cursor-pointer">
                                                        <input type="radio" class="form-radio text-danger" name="badge" value="danger" v-model="params.type" />
                                                        <span class="ltr:pl-2 rtl:pr-2">Important</span>
                                                    </label>
                                                </div>
                                            </div>
                                            <div class="mt-8 flex items-center justify-end">
                                                <button type="button" class="btn btn-outline-danger" @click="isAddEventModal = false">Cancel</button>
                                                <button type="submit" class="btn btn-primary ltr:ml-4 rtl:mr-4">
                                                    {{ params.id ? 'Update Event' : 'Create Event' }}
                                                </button>
                                            </div>
                                        </form>
                                    </div>
                                </DialogPanel>
                            </TransitionChild>
                        </div>
                    </div>
                </Dialog>
            </TransitionRoot>
        </client-only>
    </div>
</template>
<script lang="ts" setup>
    import { computed, onMounted, ref } from 'vue';
    import { TransitionRoot, TransitionChild, Dialog, DialogPanel, DialogOverlay } from '@headlessui/vue';
    import FullCalendar from '@fullcalendar/vue3';
    import dayGridPlugin from '@fullcalendar/daygrid';
    import timeGridPlugin from '@fullcalendar/timegrid';
    import interactionPlugin from '@fullcalendar/interaction';
    import Swal from 'sweetalert2';
    useHead({ title: 'Calendar' });

    const defaultParams = ref({
        id: null,
        title: '',
        start: '',
        end: '',
        description: '',
        type: 'primary',
    });
    const params = ref({
        id: null,
        title: '',
        start: '',
        end: '',
        description: '',
        type: 'primary',
    });
    const isAddEventModal = ref(false);
    const minStartDate: any = ref('');
    const minEndDate: any = ref('');
    const calendar: any = ref(null);
    const now = new Date();
    const events: any = ref([]);
    const calendarOptions = computed(() => {
        return {
            plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
            initialView: 'dayGridMonth',
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth,timeGridWeek,timeGridDay',
            },
            editable: true,
            dayMaxEvents: true,
            selectable: true,
            droppable: true,
            eventClick: (event: any) => {
                editEvent(event);
            },
            select: (event: any) => {
                editDate(event);
            },
            events: events.value,
        };
    });

    onMounted(() => {
        getEvents();
    });

    const getEvents = () => {
        events.value = [
            {
                id: 1,
                title: 'All Day Event',
                start: now.getFullYear() + '-' + getMonth(now) + '-01T14:30:00',
                end: now.getFullYear() + '-' + getMonth(now) + '-02T14:30:00',
                className: 'danger',
                description: 'Aenean fermentum quam vel sapien rutrum cursus. Vestibulum imperdiet finibus odio, nec tincidunt felis facilisis eu.',
            },
            {
                id: 2,
                title: 'Site Visit',
                start: now.getFullYear() + '-' + getMonth(now) + '-07T19:30:00',
                end: now.getFullYear() + '-' + getMonth(now) + '-08T14:30:00',
                className: 'primary',
                description: 'Etiam a odio eget enim aliquet laoreet. Vivamus auctor nunc ultrices varius lobortis.',
            },
            {
                id: 3,
                title: 'Product Lunching Event',
                start: now.getFullYear() + '-' + getMonth(now) + '-17T14:30:00',
                end: now.getFullYear() + '-' + getMonth(now) + '-18T14:30:00',
                className: 'info',
                description: 'Proin et consectetur nibh. Mauris et mollis purus. Ut nec tincidunt lacus. Nam at rutrum justo, vitae egestas dolor.',
            },
            {
                id: 4,
                title: 'Meeting',
                start: now.getFullYear() + '-' + getMonth(now) + '-12T10:30:00',
                end: now.getFullYear() + '-' + getMonth(now) + '-13T10:30:00',
                className: 'danger',
                description: 'Mauris ut mauris aliquam, fringilla sapien et, dignissim nisl. Pellentesque ornare velit non mollis fringilla.',
            },
            {
                id: 5,
                title: 'Lunch',
                start: now.getFullYear() + '-' + getMonth(now) + '-12T15:00:00',
                end: now.getFullYear() + '-' + getMonth(now) + '-13T15:00:00',
                className: 'info',
                description: 'Integer fermentum bibendum elit in egestas. Interdum et malesuada fames ac ante ipsum primis in faucibus.',
            },
            {
                id: 6,
                title: 'Conference',
                start: now.getFullYear() + '-' + getMonth(now) + '-12T21:30:00',
                end: now.getFullYear() + '-' + getMonth(now) + '-13T21:30:00',
                className: 'success',
                description:
                    'Curabitur facilisis vel elit sed dapibus. Nunc sagittis ex nec ante facilisis, sed sodales purus rhoncus. Donec est sapien, porttitor et feugiat sed, eleifend quis sapien. Sed sit amet maximus dolor.',
            },
            {
                id: 7,
                title: 'Happy Hour',
                start: now.getFullYear() + '-' + getMonth(now) + '-12T05:30:00',
                end: now.getFullYear() + '-' + getMonth(now) + '-13T05:30:00',
                className: 'info',
                description:
                    ' odio lectus, porttitor molestie scelerisque blandit, hendrerit sed ex. Aenean malesuada iaculis erat, vitae blandit nisl accumsan ut.',
            },
            {
                id: 8,
                title: 'Dinner',
                start: now.getFullYear() + '-' + getMonth(now) + '-12T20:00:00',
                end: now.getFullYear() + '-' + getMonth(now) + '-13T20:00:00',
                className: 'danger',
                description:
                    'Sed purus urna, aliquam et pharetra ut, efficitur id mi. Pellentesque ut convallis velit. Lorem ipsum dolor sit amet, consectetur adipiscing elit.',
            },
            {
                id: 9,
                title: 'Birthday Party',
                start: now.getFullYear() + '-' + getMonth(now) + '-27T20:00:00',
                end: now.getFullYear() + '-' + getMonth(now) + '-28T20:00:00',
                className: 'success',
                description:
                    'Sed purus urna, aliquam et pharetra ut, efficitur id mi. Pellentesque ut convallis velit. Lorem ipsum dolor sit amet, consectetur adipiscing elit.',
            },
            {
                id: 10,
                title: 'New Talent Event',
                start: now.getFullYear() + '-' + getMonth(now, 1) + '-24T08:12:14',
                end: now.getFullYear() + '-' + getMonth(now, 1) + '-27T22:20:20',
                className: 'danger',
                description:
                    'Sed purus urna, aliquam et pharetra ut, efficitur id mi. Pellentesque ut convallis velit. Lorem ipsum dolor sit amet, consectetur adipiscing elit.',
            },
            {
                id: 11,
                title: 'Other new',
                start: now.getFullYear() + '-' + getMonth(now, -1) + '-13T08:12:14',
                end: now.getFullYear() + '-' + getMonth(now, -1) + '-16T22:20:20',
                className: 'primary',
                description:
                    'Pellentesque ut convallis velit. Sed purus urna, aliquam et pharetra ut, efficitur id mi. Lorem ipsum dolor sit amet, consectetur adipiscing elit.',
            },
            {
                id: 13,
                title: 'Upcoming Event',
                start: now.getFullYear() + '-' + getMonth(now, 1) + '-15T08:12:14',
                end: now.getFullYear() + '-' + getMonth(now, 1) + '-18T22:20:20',
                className: 'primary',
                description:
                    'Pellentesque ut convallis velit. Sed purus urna, aliquam et pharetra ut, efficitur id mi. Lorem ipsum dolor sit amet, consectetur adipiscing elit.',
            },
        ];
    };

    const getMonth = (dt: Date, add: number = 0) => {
        let month = dt.getMonth() + 1 + add;
        const str = (month < 10 ? '0' + month : month).toString();
        return str;
    };

    const editEvent = (data: any = null) => {
        params.value = JSON.parse(JSON.stringify(defaultParams.value));
        if (data) {
            let obj = JSON.parse(JSON.stringify(data.event));
            params.value = {
                id: obj.id ? obj.id : null,
                title: obj.title ? obj.title : null,
                start: dateFormat(obj.start),
                end: dateFormat(obj.end),
                type: obj.classNames ? obj.classNames[0] : 'primary',
                description: obj.extendedProps ? obj.extendedProps.description : '',
            };
            minStartDate.value = new Date();
            minEndDate.value = dateFormat(obj.start);
        } else {
            minStartDate.value = new Date();
            minEndDate.value = new Date();
        }

        isAddEventModal.value = true;
    };

    const editDate = (data: any) => {
        let obj = {
            event: {
                start: data.start,
                end: data.end,
            },
        };
        editEvent(obj);
    };

    const dateFormat = (dt: any) => {
        dt = new Date(dt);
        const month = dt.getMonth() + 1 < 10 ? '0' + (dt.getMonth() + 1) : dt.getMonth() + 1;
        const date = dt.getDate() < 10 ? '0' + dt.getDate() : dt.getDate();
        const hours = dt.getHours() < 10 ? '0' + dt.getHours() : dt.getHours();
        const mins = dt.getMinutes() < 10 ? '0' + dt.getMinutes() : dt.getMinutes();
        dt = dt.getFullYear() + '-' + month + '-' + date + 'T' + hours + ':' + mins;
        return dt;
    };

    const saveEvent = () => {
        if (!params.value.title) {
            return true;
        }
        if (!params.value.start) {
            return true;
        }
        if (!params.value.end) {
            return true;
        }

        if (params.value.id) {
            //update event
            let event = events.value.find((d: any) => d.id == params.value.id);
            event.title = params.value.title;
            event.start = params.value.start;
            event.end = params.value.end;
            event.description = params.value.description;
            event.className = params.value.type;
        } else {
            //add event
            let maxEventId = 0;
            if (events.value) {
                maxEventId = events.value.reduce((max: number, character: any) => (character.id > max ? character.id : max), events.value[0].id);
            }

            let event = {
                id: maxEventId + 1,
                title: params.value.title,
                start: params.value.start,
                end: params.value.end,
                description: params.value.description,
                className: params.value.type,
            };
            events.value.push(event);
        }
        calendar.value.getApi(); //refresh Calendar
        showMessage('Event has been saved successfully.');
        isAddEventModal.value = false;
    };

    const startDateChange = (event: any) => {
        const dateStr = event.target.value;
        if (dateStr) {
            minEndDate.value = dateFormat(dateStr);
            params.value.end = '';
        }
    };

    const showMessage = (msg = '', type = 'success') => {
        const toast: any = Swal.mixin({
            toast: true,
            position: 'top',
            showConfirmButton: false,
            timer: 3000,
            customClass: { container: 'toast' },
        });
        toast.fire({
            icon: type,
            title: msg,
            padding: '10px 20px',
        });
    };
</script>
