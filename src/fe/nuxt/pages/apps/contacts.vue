<template>
    <div>
        <div class="flex flex-wrap items-center justify-between gap-4">
            <h2 class="text-xl">Contacts</h2>
            <div class="flex w-full flex-col gap-4 sm:w-auto sm:flex-row sm:items-center sm:gap-3">
                <div class="flex gap-3">
                    <div>
                        <button type="button" class="btn btn-primary" @click="editUser()">
                            <icon-user-plus class="ltr:mr-2 rtl:ml-2" />

                            Add Contact
                        </button>
                    </div>
                    <div>
                        <button
                            type="button"
                            class="btn btn-outline-primary p-2"
                            :class="{ 'bg-primary text-white': displayType === 'list' }"
                            @click="displayType = 'list'"
                        >
                            <icon-list-check />
                        </button>
                    </div>
                    <div>
                        <button
                            type="button"
                            class="btn btn-outline-primary p-2"
                            :class="{ 'bg-primary text-white': displayType === 'grid' }"
                            @click="displayType = 'grid'"
                        >
                            <icon-layout-grid />
                        </button>
                    </div>
                </div>

                <div class="relative">
                    <input
                        type="text"
                        placeholder="Search Contacts"
                        class="peer form-input py-2 ltr:pr-11 rtl:pl-11"
                        v-model="searchUser"
                        @keyup="searchContacts"
                    />
                    <div class="absolute top-1/2 -translate-y-1/2 peer-focus:text-primary ltr:right-[11px] rtl:left-[11px]">
                        <icon-search class="mx-auto" />
                    </div>
                </div>
            </div>
        </div>
        <div class="panel mt-5 overflow-hidden border-0 p-0">
            <template v-if="displayType === 'list'">
                <div class="table-responsive">
                    <table class="table-striped table-hover">
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Email</th>
                                <th>Location</th>
                                <th>Phone</th>
                                <th class="!text-center">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <template v-for="contact in filterdContactsList" :key="contact.id">
                                <tr>
                                    <td>
                                        <div class="flex w-max items-center">
                                            <div v-show="contact.path" class="w-max">
                                                <img
                                                    :src="`/assets/images/${contact.path}`"
                                                    class="h-8 w-8 rounded-full object-cover ltr:mr-2 rtl:ml-2"
                                                    alt="avatar"
                                                />
                                            </div>
                                            <div
                                                v-show="!contact.path && contact.name"
                                                class="grid h-8 w-8 place-content-center rounded-full bg-primary text-sm font-semibold text-white ltr:mr-2 rtl:ml-2"
                                            >
                                                {{ contact.name.charAt(0) + '' + contact.name.charAt(contact.name.indexOf(' ') + 1) }}
                                            </div>
                                            <div
                                                v-show="!contact.path && !contact.name"
                                                class="grid h-8 w-8 place-content-center rounded-full border border-gray-300 ltr:mr-2 rtl:ml-2 dark:border-gray-800"
                                            >
                                                <icon-user class="w-4.5 h-4.5" />
                                            </div>
                                            <div>{{ contact.name }}</div>
                                        </div>
                                    </td>
                                    <td>{{ contact.email }}</td>
                                    <td class="whitespace-nowrap">{{ contact.location }}</td>
                                    <td class="whitespace-nowrap">{{ contact.phone }}</td>
                                    <td>
                                        <div class="flex items-center justify-center gap-4">
                                            <button type="button" class="btn btn-sm btn-outline-primary" @click="editUser(contact)">Edit</button>
                                            <button type="button" class="btn btn-sm btn-outline-danger" @click="deleteUser(contact)">Delete</button>
                                        </div>
                                    </td>
                                </tr>
                            </template>
                        </tbody>
                    </table>
                </div>
            </template>
        </div>
        <template v-if="displayType === 'grid'">
            <div class="grid w-full grid-cols-1 gap-6 sm:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4">
                <template v-for="contact in filterdContactsList" :key="contact.id">
                    <div class="relative overflow-hidden rounded-md bg-white text-center shadow dark:bg-[#1c232f]">
                        <div class="rounded-t-md bg-white/40 bg-[url('/assets/images/notification-bg.png')] bg-cover bg-center p-6 pb-0">
                            <template v-if="contact.path">
                                <img class="mx-auto max-h-40 w-4/5 object-contain" :src="`/assets/images/${contact.path}`" />
                            </template>
                        </div>
                        <div class="relative -mt-10 px-6 pb-24">
                            <div class="rounded-md bg-white px-2 py-4 shadow-md dark:bg-gray-900">
                                <div class="text-xl">{{ contact.name }}</div>
                                <div class="text-white-dark">{{ contact.role }}</div>
                                <div class="mt-6 flex flex-wrap items-center justify-between gap-3">
                                    <div class="flex-auto">
                                        <div class="text-info">{{ contact.posts }}</div>
                                        <div>Posts</div>
                                    </div>
                                    <div class="flex-auto">
                                        <div class="text-info">{{ contact.following }}</div>
                                        <div>Following</div>
                                    </div>
                                    <div class="flex-auto">
                                        <div class="text-info">{{ contact.followers }}</div>
                                        <div>Followers</div>
                                    </div>
                                </div>
                                <div class="mt-4">
                                    <ul class="flex items-center justify-center space-x-4 rtl:space-x-reverse">
                                        <li>
                                            <a href="javascript:;" class="btn btn-outline-primary h-7 w-7 rounded-full p-0">
                                                <icon-facebook />
                                            </a>
                                        </li>
                                        <li>
                                            <a href="javascript:;" class="btn btn-outline-primary h-7 w-7 rounded-full p-0">
                                                <icon-instagram />
                                            </a>
                                        </li>
                                        <li>
                                            <a href="javascript:;" class="btn btn-outline-primary h-7 w-7 rounded-full p-0">
                                                <icon-linkedin />
                                            </a>
                                        </li>
                                        <li>
                                            <a href="javascript:;" class="btn btn-outline-primary h-7 w-7 rounded-full p-0">
                                                <icon-twitter />
                                            </a>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                            <div class="mt-6 grid grid-cols-1 gap-4 ltr:text-left rtl:text-right">
                                <div class="flex items-center">
                                    <div class="flex-none ltr:mr-2 rtl:ml-2">Email :</div>
                                    <div class="truncate text-white-dark">{{ contact.email }}</div>
                                </div>
                                <div class="flex items-center">
                                    <div class="flex-none ltr:mr-2 rtl:ml-2">Phone :</div>
                                    <div class="text-white-dark">{{ contact.phone }}</div>
                                </div>
                                <div class="flex items-center">
                                    <div class="flex-none ltr:mr-2 rtl:ml-2">Address :</div>
                                    <div class="text-white-dark">{{ contact.location }}</div>
                                </div>
                            </div>
                        </div>
                        <div class="absolute bottom-0 mt-6 flex w-full gap-4 p-6 ltr:left-0 rtl:right-0">
                            <button type="button" class="btn btn-outline-primary w-1/2" @click="editUser(contact)">Edit</button>
                            <button type="button" class="btn btn-outline-danger w-1/2" @click="deleteUser(contact)">Delete</button>
                        </div>
                    </div>
                </template>
            </div>
        </template>

        <!-- add contact modal -->
        <TransitionRoot appear :show="addContactModal" as="template">
            <Dialog as="div" @close="addContactModal = false" class="relative z-[51]">
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
                                    @click="addContactModal = false"
                                >
                                    <icon-x />
                                </button>
                                <div class="bg-[#fbfbfb] py-3 text-lg font-medium ltr:pl-5 ltr:pr-[50px] rtl:pr-5 rtl:pl-[50px] dark:bg-[#121c2c]">
                                    {{ params.id ? 'Edit Contact' : 'Add Contact' }}
                                </div>
                                <div class="p-5">
                                    <form @submit.prevent="saveUser">
                                        <div class="mb-5">
                                            <label for="name">Name</label>
                                            <input id="name" type="text" placeholder="Enter Name" class="form-input" v-model="params.name" />
                                        </div>
                                        <div class="mb-5">
                                            <label for="email">Email</label>
                                            <input id="email" type="email" placeholder="Enter Email" class="form-input" v-model="params.email" />
                                        </div>
                                        <div class="mb-5">
                                            <label for="number">Phone Number</label>
                                            <input id="number" type="text" placeholder="Enter Phone Number" class="form-input" v-model="params.phone" />
                                        </div>
                                        <div class="mb-5">
                                            <label for="occupation">Occupation</label>
                                            <input id="occupation" type="text" placeholder="Enter Occupation" class="form-input" v-model="params.role" />
                                        </div>
                                        <div class="mb-5">
                                            <label for="address">Address</label>
                                            <textarea
                                                id="address"
                                                rows="3"
                                                placeholder="Enter Address"
                                                class="form-textarea min-h-[130px] resize-none"
                                                v-model="params.location"
                                            ></textarea>
                                        </div>
                                        <div class="mt-8 flex items-center justify-end">
                                            <button type="button" class="btn btn-outline-danger" @click="addContactModal = false">Cancel</button>
                                            <button type="submit" class="btn btn-primary ltr:ml-4 rtl:mr-4">
                                                {{ params.id ? 'Update' : 'Add' }}
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
    </div>
</template>
<script lang="ts" setup>
    import { ref, onMounted } from 'vue';
    import { TransitionRoot, TransitionChild, Dialog, DialogPanel, DialogOverlay } from '@headlessui/vue';
    import Swal from 'sweetalert2';
    useHead({ title: 'Contacts' });

    const defaultParams = ref({
        id: null,
        name: '',
        email: '',
        role: '',
        phone: '',
        location: '',
    });
    const displayType = ref('list');
    const addContactModal = ref(false);
    const params = ref(JSON.parse(JSON.stringify(defaultParams.value)));
    const filterdContactsList: any = ref([]);
    const searchUser = ref('');
    const contactList = ref([
        {
            id: 1,
            path: 'profile-35.png',
            name: 'Alan Green',
            role: 'Web Developer',
            email: 'alan@mail.com',
            location: 'Boston, USA',
            phone: '+1 202 555 0197',
            posts: 25,
            followers: '5K',
            following: 500,
        },
        {
            id: 2,
            path: 'profile-35.png',
            name: 'Linda Nelson',
            role: 'Web Designer',
            email: 'linda@mail.com',
            location: 'Sydney, Australia',
            phone: '+1 202 555 0170',
            posts: 25,
            followers: '21.5K',
            following: 350,
        },
        {
            id: 3,
            path: 'profile-35.png',
            name: 'Lila Perry',
            role: 'UX/UI Designer',
            email: 'lila@mail.com',
            location: 'Miami, USA',
            phone: '+1 202 555 0105',
            posts: 20,
            followers: '21.5K',
            following: 350,
        },
        {
            id: 4,
            path: 'profile-35.png',
            name: 'Andy King',
            role: 'Project Lead',
            email: 'andy@mail.com',
            location: 'Tokyo, Japan',
            phone: '+1 202 555 0194',
            posts: 25,
            followers: '21.5K',
            following: 300,
        },
        {
            id: 5,
            path: 'profile-35.png',
            name: 'Jesse Cory',
            role: 'Web Developer',
            email: 'jesse@mail.com',
            location: 'Edinburgh, UK',
            phone: '+1 202 555 0161',
            posts: 30,
            followers: '20K',
            following: 350,
        },
        {
            id: 6,
            path: 'profile-35.png',
            name: 'Xavier',
            role: 'UX/UI Designer',
            email: 'xavier@mail.com',
            location: 'New York, USA',
            phone: '+1 202 555 0155',
            posts: 25,
            followers: '21.5K',
            following: 350,
        },
        {
            id: 7,
            path: 'profile-35.png',
            name: 'Susan',
            role: 'Project Manager',
            email: 'susan@mail.com',
            location: 'Miami, USA',
            phone: '+1 202 555 0118',
            posts: 40,
            followers: '21.5K',
            following: 350,
        },
        {
            id: 8,
            path: 'profile-35.png',
            name: 'Raci Lopez',
            role: 'Web Developer',
            email: 'traci@mail.com',
            location: 'Edinburgh, UK',
            phone: '+1 202 555 0135',
            posts: 25,
            followers: '21.5K',
            following: 350,
        },
        {
            id: 9,
            path: 'profile-35.png',
            name: 'Steven Mendoza',
            role: 'HR',
            email: 'sokol@verizon.net',
            location: 'Monrovia, US',
            phone: '+1 202 555 0100',
            posts: 40,
            followers: '21.8K',
            following: 300,
        },
        {
            id: 10,
            path: 'profile-35.png',
            name: 'James Cantrell',
            role: 'Web Developer',
            email: 'sravani@comcast.net',
            location: 'Michigan, US',
            phone: '+1 202 555 0134',
            posts: 100,
            followers: '28K',
            following: 520,
        },
        {
            id: 11,
            path: 'profile-35.png',
            name: 'Reginald Brown',
            role: 'Web Designer',
            email: 'drhyde@gmail.com',
            location: 'Entrimo, Spain',
            phone: '+1 202 555 0153',
            posts: 35,
            followers: '25K',
            following: 500,
        },
        {
            id: 12,
            path: 'profile-35.png',
            name: 'Stacey Smith',
            role: 'Chief technology officer',
            email: 'maikelnai@optonline.net',
            location: 'Lublin, Poland',
            phone: '+1 202 555 0115',
            posts: 21,
            followers: '5K',
            following: 200,
        },
    ]);

    onMounted(() => {
        searchContacts();
    });

    const searchContacts = () => {
        filterdContactsList.value = contactList.value.filter((d) => d.name.toLowerCase().includes(searchUser.value.toLowerCase()));
    };

    const editUser = (user: any = null) => {
        params.value = JSON.parse(JSON.stringify(defaultParams.value));
        if (user) {
            params.value = JSON.parse(JSON.stringify(user));
        }

        addContactModal.value = true;
    };

    const saveUser = () => {
        if (!params.value.name) {
            showMessage('Name is required.', 'error');
            return true;
        }
        if (!params.value.email) {
            showMessage('Email is required.', 'error');
            return true;
        }
        if (!params.value.phone) {
            showMessage('Phone is required.', 'error');
            return true;
        }
        if (!params.value.role) {
            showMessage('Occupation is required.', 'error');
            return true;
        }

        if (params.value.id) {
            //update user
            let user: any = contactList.value.find((d) => d.id === params.value.id);
            user.name = params.value.name;
            user.email = params.value.email;
            user.role = params.value.role;
            user.phone = params.value.phone;
            user.location = params.value.location;
        } else {
            //add user
            let maxUserId = contactList.value.length
                ? contactList.value.reduce((max, character) => (character.id > max ? character.id : max), contactList.value[0].id)
                : 0;

            let user = {
                id: maxUserId + 1,
                path: 'profile-35.png',
                name: params.value.name,
                email: params.value.email,
                role: params.value.role,
                phone: params.value.phone,
                location: params.value.location,
                posts: 20,
                followers: '5K',
                following: 500,
            };
            contactList.value.splice(0, 0, user);
            searchContacts();
        }

        showMessage('User has been saved successfully.');
        addContactModal.value = false;
    };

    const deleteUser = (user: any = null) => {
        contactList.value = contactList.value.filter((d) => d.id != user.id)
        searchContacts();
        showMessage('User has been deleted successfully.');
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
