<template>
    <div>
        <div>
            <button type="button" class="btn btn-primary flex" @click="addEditProject()">
                <icon-plus class="w-5 h-5 ltr:mr-3 rtl:ml-3" />
                Add Project
            </button>
        </div>

        <!-- project list -->
        <div class="relative pt-5">
            <div class="-mx-2 h-full">
                <div class="flex flex-nowrap items-start gap-5 overflow-x-auto px-2 pb-2">
                    <template v-for="project in projectList" :key="project.id">
                        <div class="panel w-80 flex-none">
                            <div class="mb-5 flex justify-between">
                                <h4 class="text-base font-semibold">{{ project.title }}</h4>

                                <div class="flex items-center">
                                    <button type="button" class="hover:text-primary ltr:mr-2 rtl:ml-2" @click="addEditTask(project.id)">
                                        <icon-plus-circle />
                                    </button>
                                    <div class="dropdown">
                                        <client-only>
                                            <Popper :placement="store.rtlClass === 'rtl' ? 'bottom-start' : 'bottom-end'" offsetDistance="0">
                                                <button type="button" class="hover:text-primary">
                                                    <icon-horizontal-dots class="opacity-70 hover:opacity-100" />
                                                </button>
                                                <template #content="{ close }">
                                                    <ul @click="close()" class="whitespace-nowrap text-black dark:text-white-dark">
                                                        <li>
                                                            <a href="javascript:;" @click="addEditProject(project)">Edit </a>
                                                        </li>
                                                        <li>
                                                            <a href="javascript:;" @click="deleteProject(project)">Delete </a>
                                                        </li>
                                                        <li>
                                                            <a href="javascript:;" @click="clearProjects(project)">Clear All </a>
                                                        </li>
                                                    </ul>
                                                </template>
                                            </Popper>
                                        </client-only>
                                    </div>
                                </div>
                            </div>

                            <!-- task list -->
                            <draggable
                                class="connect-sorting-content min-h-[150px]"
                                group="default"
                                ghost-class="sortable-ghost"
                                drag-class="sortable-drag"
                                :animation="200"
                            >
                                <div class="sortable-list" v-for="task in project.tasks" :key="project.id + '' + task.id">
                                    <div class="mb-5 cursor-move space-y-3 rounded-md bg-[#f4f4f4] p-3 pb-5 shadow dark:bg-white-dark/20">
                                        <template v-if="task.image">
                                            <img src="/assets/images/carousel1.jpeg" alt="images" class="h-32 w-full rounded-md object-cover" />
                                        </template>
                                        <div class="text-base font-medium">{{ task.title }}</div>
                                        <p class="break-all">{{ task.description }}</p>
                                        <div class="flex flex-wrap items-center gap-2">
                                            <template v-if="task.tags?.length">
                                                <template v-for="(tag, i) in task.tags" :key="i">
                                                    <div class="btn btn-outline-primary flex px-2 py-1">
                                                        <icon-tag class="shrink-0" />

                                                        <span class="ltr:ml-2 rtl:mr-2">{{ tag }}</span>
                                                    </div>
                                                </template>
                                            </template>
                                            <template v-else>
                                                <div class="btn flex px-2 py-1 text-white-dark shadow-none dark:border-white-dark/50">
                                                    <icon-tag class="shrink-0" />
                                                    <span class="ltr:ml-2 rtl:mr-2">No Tags</span>
                                                </div>
                                            </template>
                                        </div>
                                        <div class="flex items-center justify-between">
                                            <div class="flex items-center font-medium hover:text-primary">
                                                <icon-calendar class="ltr:mr-3 rtl:ml-3 shrink-0" />
                                                <span>{{ task.date }}</span>
                                            </div>
                                            <div class="flex items-center">
                                                <button type="button" class="hover:text-info" @click="addEditTask(project.id, task)">
                                                    <icon-edit class="ltr:mr-3 rtl:ml-3" />
                                                </button>
                                                <button type="button" class="hover:text-danger" @click="deleteConfirmModal(project.id, task)">
                                                    <icon-trash-lines />
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </draggable>

                            <div class="pt-3">
                                <button type="button" class="btn btn-primary mx-auto" @click="addEditTask(project.id)">
                                    <icon-plus />
                                    Add Task
                                </button>
                            </div>
                        </div>
                    </template>
                </div>
            </div>
        </div>

        <!-- add project modal -->
        <TransitionRoot appear :show="isAddProjectModal" as="template">
            <Dialog as="div" @close="isAddProjectModal = false" class="relative z-[51]">
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
                                    @click="isAddProjectModal = false"
                                >
                                    <icon-x />
                                </button>
                                <div class="bg-[#fbfbfb] py-3 text-lg font-medium ltr:pl-5 ltr:pr-[50px] rtl:pl-[50px] rtl:pr-5 dark:bg-[#121c2c]">
                                    {{ params.id ? 'Edit Project' : 'Add Project' }}
                                </div>
                                <div class="p-5">
                                    <form @submit.prevent="saveProject">
                                        <div class="grid gap-5">
                                            <div>
                                                <label for="title">Name</label>
                                                <input id="title" v-model="params.title" type="text" class="form-input mt-1" placeholder="Enter Name" />
                                            </div>
                                        </div>

                                        <div class="mt-8 flex items-center justify-end">
                                            <button type="button" class="btn btn-outline-danger" @click="isAddProjectModal = false">Cancel</button>
                                            <button type="submit" class="btn btn-primary ltr:ml-4 rtl:mr-4">{{ params.id ? 'Update' : 'Add' }}</button>
                                        </div>
                                    </form>
                                </div>
                            </DialogPanel>
                        </TransitionChild>
                    </div>
                </div>
            </Dialog>
        </TransitionRoot>

        <!-- add task modal -->
        <TransitionRoot appear :show="isAddTaskModal" as="template">
            <Dialog as="div" @close="isAddTaskModal = false" class="relative z-[51]">
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
                                    @click="isAddTaskModal = false"
                                >
                                    <icon-x />
                                </button>
                                <div class="bg-[#fbfbfb] py-3 text-lg font-medium ltr:pl-5 ltr:pr-[50px] rtl:pl-[50px] rtl:pr-5 dark:bg-[#121c2c]">
                                    {{ paramsTask.id ? 'Edit Task' : 'Add Task' }}
                                </div>
                                <div class="p-5">
                                    <form @submit.prevent="saveTask">
                                        <div class="grid gap-5">
                                            <div>
                                                <label for="taskTitle">Name</label>
                                                <input id="taskTitle" v-model="paramsTask.title" type="text" class="form-input" placeholder="Enter Name" />
                                            </div>

                                            <div>
                                                <label for="taskTag">Tag</label>
                                                <input id="taskTag" v-model="paramsTask.tags" type="text" class="form-input" placeholder="Enter Tag" />
                                            </div>
                                            <div>
                                                <label for="taskdesc">Description</label>
                                                <textarea
                                                    id="taskdesc"
                                                    v-model="paramsTask.description"
                                                    class="form-textarea min-h-[130px]"
                                                    placeholder="Enter Description"
                                                ></textarea>
                                            </div>
                                        </div>

                                        <div class="mt-8 flex items-center justify-end">
                                            <button type="button" class="btn btn-outline-danger" @click="isAddTaskModal = false">Cancel</button>
                                            <button type="submit" class="btn btn-primary ltr:ml-4 rtl:mr-4">{{ paramsTask.id ? 'Update' : 'Add' }}</button>
                                        </div>
                                    </form>
                                </div>
                            </DialogPanel>
                        </TransitionChild>
                    </div>
                </div>
            </Dialog>
        </TransitionRoot>

        <!-- delete task modal -->
        <TransitionRoot appear :show="isDeleteModal" as="template">
            <Dialog as="div" @close="isDeleteModal = false" class="relative z-[51]">
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
                                    @click="isDeleteModal = false"
                                >
                                    <icon-x />
                                </button>
                                <div class="bg-[#fbfbfb] py-3 text-lg font-medium ltr:pl-5 ltr:pr-[50px] rtl:pl-[50px] rtl:pr-5 dark:bg-[#121c2c]">
                                    Delete Task
                                </div>
                                <div class="p-5 text-center">
                                    <div class="mx-auto w-fit rounded-full bg-danger p-4 text-white ring-4 ring-danger/30">
                                        <icon-trash-lines />
                                    </div>
                                    <div class="mx-auto mt-5 text-base sm:w-3/4">Are you sure you want to delete Task?</div>

                                    <div class="mt-8 flex items-center justify-center">
                                        <button type="button" class="btn btn-outline-danger" @click="isDeleteModal = false">Cancel</button>
                                        <button type="button" class="btn btn-primary ltr:ml-4 rtl:mr-4" @click="deleteTask()">Delete</button>
                                    </div>
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
    import { ref } from 'vue';
    import { TransitionRoot, TransitionChild, Dialog, DialogPanel, DialogOverlay } from '@headlessui/vue';
    import { VueDraggableNext as draggable } from 'vue-draggable-next';
    import Swal from 'sweetalert2';
    import { useAppStore } from '@/stores/index';
    useHead({ title: 'Scrumboard' });
    const store = useAppStore();
    const params = ref({
        id: null,
        title: '',
    });
    const paramsTask = ref({
        projectId: null,
        id: null,
        title: '',
        description: '',
        tags: '',
    });
    const selectedTask: any = ref(null);
    const isAddProjectModal = ref(false);
    const isAddTaskModal = ref(false);
    const isDeleteModal = ref(false);
    const projectList: any = ref([
        {
            id: 1,
            title: 'In Progress',
            tasks: [
                {
                    projectId: 1,
                    id: 1,
                    title: 'Creating a new Portfolio on Dribble',
                    description: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit.',
                    image: true,
                    date: ' 08 Aug, 2020',
                    tags: ['designing'],
                },
                {
                    projectId: 1,
                    id: 2,
                    title: 'Singapore Team Meet',
                    description: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit.',
                    date: ' 09 Aug, 2020',
                    tags: ['meeting'],
                },
            ],
        },
        {
            id: 2,
            title: 'Pending',
            tasks: [
                {
                    projectId: 2,
                    id: 1,
                    title: 'Plan a trip to another country',
                    description: '',
                    date: ' 10 Sep, 2020',
                },
            ],
        },
        {
            id: 3,
            title: 'Complete',
            tasks: [
                {
                    projectId: 3,
                    id: 1,
                    title: 'Dinner with Kelly Young',
                    description: '',
                    date: ' 08 Aug, 2020',
                },
                {
                    projectId: 3,
                    id: 2,
                    title: 'Launch New SEO Wordpress Theme ',
                    description: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.',
                    date: ' 09 Aug, 2020',
                },
            ],
        },
        {
            id: 4,
            title: 'Working',
            tasks: [],
        },
    ]);

    const addEditProject = (project: any = null) => {
        setTimeout(() => {
            params.value = {
                id: null,
                title: '',
            };
            if (project) {
                params.value = JSON.parse(JSON.stringify(project));
            }

            isAddProjectModal.value = true;
        });
    };

    const saveProject = () => {
        if (!params.value.title) {
            showMessage('Title is required.', 'error');
            return false;
        }

        if (params.value.id) {
            //update project
            const project = projectList.value.find((d: any) => d.id === params.value.id);
            project.title = params.value.title;
        } else {
            //add project
            const lastId = projectList.value.length
                ? projectList.value.reduce((max: number, obj: any) => (obj.id > max ? obj.id : max), projectList.value[0].id)
                : 0;

            const project = {
                id: lastId + 1,
                title: params.value.title,
                tasks: [],
            };
            projectList.value.push(project);
        }

        showMessage('Project has been saved successfully.');
        isAddProjectModal.value = false;
    };

    const deleteProject = (project: any) => {
        projectList.value = projectList.value.filter((d: any) => d.id != project.id);
        showMessage('Project has been deleted successfully.');
    };

    const clearProjects = (project: any) => {
        project.tasks = [];
    };

    // task
    const addEditTask = (projectId: any, task: any = null) => {
        paramsTask.value = {
            projectId: null,
            id: null,
            title: '',
            description: '',
            tags: '',
        };
        if (task) {
            paramsTask.value = JSON.parse(JSON.stringify(task));
            paramsTask.value.tags = paramsTask.value.tags ? paramsTask.value.tags.toString() : '';
        }
        paramsTask.value.projectId = projectId;
        isAddTaskModal.value = true;
    };

    const saveTask = () => {
        if (!paramsTask.value.title) {
            showMessage('Title is required.', 'error');
            return false;
        }

        const project = projectList.value.find((d: any) => d.id === paramsTask.value.projectId);
        if (paramsTask.value.id) {
            //update task
            const task = project.tasks.find((d: any) => d.id === paramsTask.value.id);
            task.title = paramsTask.value.title;
            task.description = paramsTask.value.description;
            task.tags = paramsTask.value.tags?.length > 0 ? paramsTask.value.tags.split(',') : [];
        } else {
            //add task
            let maxid = 0;
            if (project.tasks?.length) {
                maxid = project.tasks.reduce((max: number, obj: any) => (obj.id > max ? obj.id : max), project.tasks[0].id);
            }

            const today = new Date();
            const dd = String(today.getDate()).padStart(2, '0');
            const mm = String(today.getMonth()); //January is 0!
            const yyyy = today.getFullYear();
            const monthNames: any = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

            const task = {
                projectId: paramsTask.value.projectId,
                id: maxid + 1,
                title: paramsTask.value.title,
                description: paramsTask.value.description,
                date: dd + ' ' + monthNames[mm] + ', ' + yyyy,
                tags: paramsTask.value.tags?.length > 0 ? paramsTask.value.tags.split(',') : [],
            };

            project.tasks.push(task);
        }

        showMessage('Task has been saved successfully.');
        isAddTaskModal.value = false;
    };

    const deleteConfirmModal = (projectId: any, task: any = null) => {
        selectedTask.value = task;
        setTimeout(() => {
            isDeleteModal.value = true;
        }, 10);
    };

    const deleteTask = () => {
        let project = projectList.value.find((d: any) => d.id === selectedTask.value.projectId);
        project.tasks = project.tasks.filter((d: any) => d.id != selectedTask.value.id);

        showMessage('Task has been deleted successfully.');
        isDeleteModal.value = false;
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
