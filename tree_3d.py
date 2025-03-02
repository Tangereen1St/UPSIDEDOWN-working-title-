import numpy as np
from OpenGL.GL import *
from OpenGL.GLUT import *
from OpenGL.GLU import *
import random
import math

class Branch:
    def __init__(self, pos, dir, length, thickness, level):
        self.pos = np.array(pos)
        # Normalize the direction vector
        self.dir = np.array(dir) / np.linalg.norm(np.array(dir))
        self.length = length
        self.thickness = thickness
        self.level = level
        self.children = []

class Tree:
    def __init__(self):
        self.branches = []
        self.angle = 0
        
    def generate(self, pos=(0,0,0), length=2.0, thickness=0.15, max_level=5):
        # Start with trunk
        trunk = Branch(pos, (0,1,0), length, thickness, 0)
        self.branches.append(trunk)
        self._generate_branches(trunk, max_level)
    
    def _generate_branches(self, parent, max_level):
        if parent.level >= max_level:
            return
            
        # More branches for lower levels, fewer for higher levels
        num_branches = random.randint(3, 5) if parent.level < 2 else random.randint(2, 3)
        
        for _ in range(num_branches):
            # Calculate new branch direction with some randomness
            angle = random.uniform(0, 2 * math.pi)
            # Less spread near trunk, more spread higher up
            spread = random.uniform(30, 60) if parent.level > 1 else random.uniform(20, 40)
            
            new_dir = self._rotate_vector(
                parent.dir,
                angle,
                math.radians(spread)
            )
            
            # New branch properties
            new_pos = parent.pos + parent.dir * parent.length
            # More dramatic length reduction higher up
            length_factor = 0.8 if parent.level < 2 else 0.6
            new_length = parent.length * length_factor
            new_thickness = parent.thickness * 0.7
            
            # Create new branch
            new_branch = Branch(
                new_pos,
                new_dir,
                new_length,
                new_thickness,
                parent.level + 1
            )
            
            parent.children.append(new_branch)
            self.branches.append(new_branch)
            
            # Recursively generate more branches
            self._generate_branches(new_branch, max_level)
    
    def _rotate_vector(self, vector, angle, spread):
        # Create rotation matrix and apply it
        rotation = np.array([
            [math.cos(angle), 0, math.sin(angle)],
            [0, math.cos(spread), -math.sin(spread)],
            [-math.sin(angle), math.sin(spread), math.cos(angle)]
        ])
        return np.dot(rotation, vector)
    
    def draw(self):
        glPushMatrix()
        
        # Rotate tree for animation
        glRotatef(self.angle, 0, 1, 0)
        
        for branch in self.branches:
            self._draw_branch(branch)
            
        glPopMatrix()
        self.angle += 0.5
    
    def _draw_branch(self, branch):
        glPushMatrix()
        
        # Darker brown for trunk, lighter for branches
        if branch.level == 0:
            glColor3f(0.4, 0.2, 0.0)  # Dark brown
        else:
            glColor3f(0.6, 0.4, 0.1)  # Lighter brown
        glTranslatef(*branch.pos)
        
        # Orient cylinder along branch direction
        axis = np.cross([0,1,0], branch.dir)
        # Normalize the dot product to ensure it's in [-1, 1] range
        dot_product = np.clip(np.dot([0,1,0], branch.dir), -1.0, 1.0)
        angle = math.degrees(math.acos(dot_product))
        
        if not np.allclose(axis, 0):
            glRotatef(angle, *axis)
        
        # Draw cylinder with more segments for smoother look
        quad = gluNewQuadric()
        gluCylinder(quad, branch.thickness, branch.thickness * 0.8,
                   branch.length, 12, 3)
        
        # Draw leaves at end branches
        if not branch.children:
            self._draw_leaves(branch)
        
        glPopMatrix()
    
    def _draw_leaves(self, branch):
        glPushMatrix()
        glTranslatef(0, branch.length, 0)
        
        # Slightly vary leaf colors
        green = random.uniform(0.6, 0.8)
        glColor3f(0.0, green, 0.0)
        
        # Draw leaf cluster as sphere
        quad = gluNewQuadric()
        gluSphere(quad, branch.thickness * 4, 12, 12)
        
        glPopMatrix()

def init():
    glEnable(GL_DEPTH_TEST)
    glEnable(GL_LIGHTING)
    glEnable(GL_LIGHT0)
    glLightfv(GL_LIGHT0, GL_POSITION, [1, 1, 1, 0])
    glLightfv(GL_LIGHT0, GL_AMBIENT, [0.3, 0.3, 0.3, 1.0])
    glLightfv(GL_LIGHT0, GL_DIFFUSE, [0.7, 0.7, 0.7, 1.0])
    glEnable(GL_COLOR_MATERIAL)

def display():
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
    glLoadIdentity()
    
    # Position camera
    gluLookAt(3, 2, 3, 0, 1, 0, 0, 1, 0)
    
    # Draw tree
    tree.draw()
    
    glutSwapBuffers()

def reshape(width, height):
    glViewport(0, 0, width, height)
    glMatrixMode(GL_PROJECTION)
    glLoadIdentity()
    gluPerspective(45, width/height, 0.1, 50.0)
    glMatrixMode(GL_MODELVIEW)

def animate(value):
    glutPostRedisplay()
    glutTimerFunc(16, animate, 0)

# Initialize GLUT and create window
glutInit()
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH)
glutInitWindowSize(800, 600)
glutCreateWindow(b"3D Tree Generator")

# Initialize OpenGL
init()

# Create and generate tree
tree = Tree()
tree.generate(max_level=5)  # Increased levels for more detail

# Set up callbacks
glutDisplayFunc(display)
glutReshapeFunc(reshape)
glutTimerFunc(0, animate, 0)

# Start main loop
glutMainLoop() 