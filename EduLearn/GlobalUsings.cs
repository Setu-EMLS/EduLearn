// The area namespaces EduLearn.Areas.Student / .Teacher shadow the entity class names
// inside EduLearn.* namespaces, so the entities are referenced through these aliases.
global using StudentEntity = EduLearn.Areas.Student.Models.Student;
global using TeacherEntity = EduLearn.Areas.Teacher.Models.Teacher;
